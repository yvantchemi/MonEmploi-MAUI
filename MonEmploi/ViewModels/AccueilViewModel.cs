using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

/// <summary>Taux d'occupation hebdomadaire d'une salle.</summary>
public class OccupationSalle
{
    public OccupationSalle(string nom, double heures, double heuresDisponibles)
    {
        Nom = nom;
        Heures = heures;
        Taux = heuresDisponibles <= 0 ? 0 : Math.Min(1, heures / heuresDisponibles);
    }

    public string Nom { get; }
    public double Heures { get; }
    public double Taux { get; }
    public string Texte => $"{Heures:0.#} h · {Taux:P0}";
}

/// <summary>Tableau de bord : chiffres clés, conflits, séances du jour et occupation des salles.</summary>
public class AccueilViewModel : BaseViewModel
{
    /// <summary>Plage d'ouverture retenue pour le calcul d'occupation : 6 jours × 10 h (8 h – 18 h).</summary>
    private const double HeuresOuverture = 60;

    private readonly DataStore _store;
    private readonly ConflictService _conflits;

    public AccueilViewModel(DataStore store, ConflictService conflits)
    {
        _store = store;
        _conflits = conflits;
        _store.DonneesModifiees += (_, _) => Rafraichir();

        NouvelleSeanceCommand = new Command(async () => await Shell.Current.GoToAsync("seance"));
        OuvrirCommand = new Command<SeanceAffichage>(async s =>
        {
            if (s is not null)
                await Shell.Current.GoToAsync($"seance?id={s.Seance.Id}");
        });
        ReinitialiserCommand = new Command(async () =>
        {
            if (await Confirmer("Données de démonstration",
                    "Remplacer toutes les données par le jeu de démonstration ?"))
                _store.ReinitialiserDemo();
        });

        Rafraichir();
    }

    public int NbSalles { get; private set; }
    public int NbEnseignants { get; private set; }
    public int NbGroupes { get; private set; }
    public int NbSeances { get; private set; }
    public int NbConflits { get; private set; }
    public string HeuresSemaine { get; private set; } = string.Empty;

    public bool AConflits => NbConflits > 0;
    public string TexteConflits => AConflits
        ? $"{NbConflits} séance(s) en conflit à corriger"
        : "Aucun conflit dans l'emploi du temps";

    public string TitreJour { get; private set; } = string.Empty;
    public ObservableCollection<SeanceAffichage> SeancesDuJour { get; } = new();
    public bool AucuneSeanceAujourdhui => SeancesDuJour.Count == 0;

    public ObservableCollection<OccupationSalle> Occupations { get; } = new();

    public ICommand NouvelleSeanceCommand { get; }
    public ICommand OuvrirCommand { get; }
    public ICommand ReinitialiserCommand { get; }

    public void Rafraichir()
    {
        NbSalles = _store.Salles.Count;
        NbEnseignants = _store.Enseignants.Count;
        NbGroupes = _store.Groupes.Count;
        NbSeances = _store.Seances.Count;

        var enConflit = _conflits.SeancesEnConflit();
        NbConflits = enConflit.Count;
        HeuresSemaine = $"{_store.Seances.Sum(s => (s.Fin - s.Debut).TotalHours):0.#} h de cours par semaine";

        // Séances du jour (le dimanche, on affiche celles du lundi)
        var aujourdhui = DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? DayOfWeek.Monday : DateTime.Today.DayOfWeek;
        TitreJour = DateTime.Today.DayOfWeek == DayOfWeek.Sunday
            ? "Lundi prochain"
            : $"Aujourd'hui — {Jours.Nom(aujourdhui)}";

        SeancesDuJour.Clear();
        foreach (var s in _store.Seances.Where(s => s.Jour == aujourdhui).OrderBy(s => s.Debut))
        {
            var a = _store.Decrire(s);
            a.EnConflit = enConflit.Contains(s.Id);
            SeancesDuJour.Add(a);
        }

        Occupations.Clear();
        foreach (var salle in _store.Salles.OrderBy(s => s.Nom))
        {
            var heures = _store.Seances.Where(s => s.SalleId == salle.Id).Sum(s => (s.Fin - s.Debut).TotalHours);
            Occupations.Add(new OccupationSalle(salle.Nom, heures, HeuresOuverture));
        }

        foreach (var nom in new[] { nameof(NbSalles), nameof(NbEnseignants), nameof(NbGroupes), nameof(NbSeances),
                                    nameof(NbConflits), nameof(AConflits), nameof(TexteConflits), nameof(HeuresSemaine),
                                    nameof(TitreJour), nameof(AucuneSeanceAujourdhui) })
            OnPropertyChanged(nom);
    }
}
