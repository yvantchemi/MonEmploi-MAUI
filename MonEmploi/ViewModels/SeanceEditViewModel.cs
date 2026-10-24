using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SeanceEditViewModel : BaseViewModel
{
    private readonly DataStore _store;

    private Seance _seance = new();
    private string _matiere = string.Empty;
    private Enseignant? _enseignant;
    private Groupe? _groupe;
    private Salle? _salle;
    private int _jourIndex;
    private TimeSpan _debut = new(8, 0, 0);
    private TimeSpan _fin = new(10, 0, 0);

    public SeanceEditViewModel(DataStore store)
    {
        _store = store;
        EnregistrerCommand = new Command(async () => await Enregistrer());
        AnnulerCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // ---------- Listes de choix ----------
    public ObservableCollection<Enseignant> Enseignants => _store.Enseignants;
    public ObservableCollection<Groupe> Groupes => _store.Groupes;
    public ObservableCollection<Salle> Salles => _store.Salles;
    public IReadOnlyList<string> JoursNoms => Jours.Noms;

    public string Titre => "Nouvelle séance";

    // ---------- Champs ----------
    public string Matiere { get => _matiere; set => SetProperty(ref _matiere, value); }
    public Enseignant? EnseignantChoisi { get => _enseignant; set => SetProperty(ref _enseignant, value); }
    public Groupe? GroupeChoisi { get => _groupe; set => SetProperty(ref _groupe, value); }
    public Salle? SalleChoisie { get => _salle; set => SetProperty(ref _salle, value); }
    public int JourIndex { get => _jourIndex; set => SetProperty(ref _jourIndex, value); }
    public TimeSpan Debut { get => _debut; set => SetProperty(ref _debut, value); }
    public TimeSpan Fin { get => _fin; set => SetProperty(ref _fin, value); }

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

        _store.EnregistrerSeance(ConstruireSeance());
        await Shell.Current.GoToAsync("..");
    }
}
