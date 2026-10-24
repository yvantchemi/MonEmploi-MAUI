using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SeanceEditViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private readonly ConflictService _conflits;

    private Seance _seance = new();
    private string _matiere = string.Empty;
    private Enseignant? _enseignant;
    private Groupe? _groupe;
    private Salle? _salle;
    private int _jourIndex;
    private TimeSpan _debut = new(8, 0, 0);
    private TimeSpan _fin = new(10, 0, 0);
    private string _etat = "Renseignez la séance pour lancer la vérification.";

    public SeanceEditViewModel(DataStore store, ConflictService conflits)
    {
        _store = store;
        _conflits = conflits;
        EnregistrerCommand = new Command(async () => await Enregistrer());
        AnnulerCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // ---------- Listes de choix ----------
    public ObservableCollection<Enseignant> Enseignants => _store.Enseignants;
    public ObservableCollection<Groupe> Groupes => _store.Groupes;
    public ObservableCollection<Salle> Salles => _store.Salles;
    public IReadOnlyList<string> JoursNoms => Jours.Noms;

    public string Titre => "Nouvelle séance";

    // ---------- Champs (chaque modification relance la détection) ----------
    public string Matiere { get => _matiere; set => SetProperty(ref _matiere, value); }
    public Enseignant? EnseignantChoisi { get => _enseignant; set { if (SetProperty(ref _enseignant, value)) Verifier(); } }
    public Groupe? GroupeChoisi { get => _groupe; set { if (SetProperty(ref _groupe, value)) Verifier(); } }
    public Salle? SalleChoisie { get => _salle; set { if (SetProperty(ref _salle, value)) Verifier(); } }
    public int JourIndex { get => _jourIndex; set { if (SetProperty(ref _jourIndex, value)) Verifier(); } }
    public TimeSpan Debut { get => _debut; set { if (SetProperty(ref _debut, value)) Verifier(); } }
    public TimeSpan Fin { get => _fin; set { if (SetProperty(ref _fin, value)) Verifier(); } }

    // ---------- Résultat de la vérification ----------
    public ObservableCollection<Conflit> Conflits { get; } = new();
    public bool AConflits => Conflits.Count > 0;
    public bool SansConflit => !AConflits;
    public string Etat { get => _etat; private set => SetProperty(ref _etat, value); }

    public ICommand EnregistrerCommand { get; }
    public ICommand AnnulerCommand { get; }

    private Seance ConstruireSeance() => new()
    {
        Id = _seance.Id,
        Matiere = Matiere.Trim(),
        EnseignantId = EnseignantChoisi?.Id ?? string.Empty,
        GroupeId = GroupeChoisi?.Id ?? string.Empty,
        SalleId = SalleChoisie?.Id ?? string.Empty,
        Jour = Jours.DepuisIndex(JourIndex),
        Debut = Debut,
        Fin = Fin
    };

    /// <summary>Détection en temps réel des conflits de salle, d'enseignant et de groupe.</summary>
    private void Verifier()
    {
        Conflits.Clear();

        if (Fin <= Debut)
        {
            Etat = "L'heure de fin doit être postérieure à l'heure de début.";
        }
        else
        {
            foreach (var conflit in _conflits.Detecter(ConstruireSeance()))
                Conflits.Add(conflit);

            Etat = AConflits
                ? $"{Conflits.Count} conflit(s) détecté(s) : modifiez le créneau, la salle, l'enseignant ou le groupe."
                : "Aucun conflit : le créneau est libre.";
        }

        OnPropertyChanged(nameof(AConflits));
        OnPropertyChanged(nameof(SansConflit));
    }

    private string? Valider()
    {
        if (string.IsNullOrWhiteSpace(Matiere)) return "Indiquez la matière.";
        if (EnseignantChoisi is null) return "Choisissez un enseignant.";
        if (GroupeChoisi is null) return "Choisissez un groupe.";
        if (SalleChoisie is null) return "Choisissez une salle.";
        if (Fin <= Debut) return "L'heure de fin doit être postérieure à l'heure de début.";
        return null;
    }

    private async Task Enregistrer()
    {
        var erreur = Valider();
        if (erreur is not null)
        {
            await AfficherMessage("Séance incomplète", erreur);
            return;
        }

        Verifier();
        if (AConflits)
        {
            var detail = string.Join("\n• ", Conflits.Select(c => c.Message));
            await AfficherMessage("Enregistrement refusé", $"Cette séance entre en conflit :\n• {detail}");
            return;
        }

        _store.EnregistrerSeance(ConstruireSeance());
        await Shell.Current.GoToAsync("..");
    }
}
