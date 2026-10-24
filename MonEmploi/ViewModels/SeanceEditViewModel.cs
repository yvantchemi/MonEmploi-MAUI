using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SeanceEditViewModel : BaseViewModel, IQueryAttributable
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
        SupprimerCommand = new Command(async () => await Supprimer());
        SuggererSalleCommand = new Command(async () => await SuggererSalle());
    }

    /// <summary>Reçoit l'identifiant de la séance à modifier (route « seance?id=... »).</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var valeur) && _store.TrouverSeance(valeur?.ToString()) is { } existante)
        {
            _seance = existante.Cloner();
            EstModification = true;

            Matiere = _seance.Matiere;
            _enseignant = _store.TrouverEnseignant(_seance.EnseignantId);
            _groupe = _store.TrouverGroupe(_seance.GroupeId);
            _salle = _store.TrouverSalle(_seance.SalleId);
            _jourIndex = Jours.Index(_seance.Jour);
            _debut = _seance.Debut;
            _fin = _seance.Fin;

            OnPropertyChanged(nameof(EnseignantChoisi));
            OnPropertyChanged(nameof(GroupeChoisi));
            OnPropertyChanged(nameof(SalleChoisie));
            OnPropertyChanged(nameof(JourIndex));
            OnPropertyChanged(nameof(Debut));
            OnPropertyChanged(nameof(Fin));
            Verifier();
        }
        else if (query.TryGetValue("jour", out var jour) && int.TryParse(jour?.ToString(), out var index))
        {
            JourIndex = index;
        }

        OnPropertyChanged(nameof(Titre));
        OnPropertyChanged(nameof(EstModification));
    }

    public bool EstModification { get; private set; }

    // ---------- Listes de choix ----------
    public ObservableCollection<Enseignant> Enseignants => _store.Enseignants;
    public ObservableCollection<Groupe> Groupes => _store.Groupes;
    public ObservableCollection<Salle> Salles => _store.Salles;
    public IReadOnlyList<string> JoursNoms => Jours.Noms;

    public string Titre => EstModification ? "Modifier la séance" : "Nouvelle séance";

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

    /// <summary>Avertissement non bloquant : effectif du groupe supérieur à la capacité de la salle.</summary>
    public string Avertissement
    {
        get
        {
            if (GroupeChoisi is null || SalleChoisie is null || GroupeChoisi.Effectif <= SalleChoisie.Capacite)
                return string.Empty;
            return $"Capacité insuffisante : {SalleChoisie.Nom} compte {SalleChoisie.Capacite} places pour " +
                   $"{GroupeChoisi.Effectif} étudiants ({GroupeChoisi.Nom}).";
        }
    }

    public bool AAvertissement => !string.IsNullOrEmpty(Avertissement);

    public ICommand SuggererSalleCommand { get; }

    /// <summary>Propose les salles libres sur le créneau et assez grandes pour le groupe.</summary>
    private async Task SuggererSalle()
    {
        if (Fin <= Debut)
        {
            await AfficherMessage("Créneau invalide", "Corrigez d'abord les heures de début et de fin.");
            return;
        }

        var effectif = GroupeChoisi?.Effectif ?? 0;
        var libres = _conflits.SallesLibres(ConstruireSeance(), effectif);

        if (libres.Count == 0)
        {
            await AfficherMessage("Aucune salle libre",
                "Aucune salle assez grande n'est disponible sur ce créneau. Essayez un autre horaire.");
            return;
        }

        var choix = await Shell.Current.DisplayActionSheet(
            $"Salles libres ({libres.Count})", "Annuler", null,
            libres.Select(s => $"{s.Nom} — {s.Capacite} places").ToArray());

        var salle = libres.FirstOrDefault(s => choix == $"{s.Nom} — {s.Capacite} places");
        if (salle is not null)
            SalleChoisie = salle;
    }

    public ICommand EnregistrerCommand { get; }
    public ICommand AnnulerCommand { get; }
    public ICommand SupprimerCommand { get; }

    private async Task Supprimer()
    {
        if (!EstModification) return;
        if (!await Confirmer("Supprimer", $"Supprimer la séance « {_seance.Matiere} » ?")) return;

        _store.SupprimerSeance(_seance);
        await Shell.Current.GoToAsync("..");
    }

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
        OnPropertyChanged(nameof(Avertissement));
        OnPropertyChanged(nameof(AAvertissement));
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

        if (AAvertissement && !await Confirmer("Capacité insuffisante", $"{Avertissement}\n\nEnregistrer quand même ?"))
            return;

        _store.EnregistrerSeance(ConstruireSeance());
        await Shell.Current.GoToAsync("..");
    }
}
