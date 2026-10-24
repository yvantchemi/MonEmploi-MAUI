using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

/// <summary>Emploi du temps filtré par enseignant, par groupe ou par salle.</summary>
public class PlanningViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private readonly ConflictService _conflits;

    private int _modeIndex;
    private ElementChoix? _element;
    private string _resume = string.Empty;

    public PlanningViewModel(DataStore store, ConflictService conflits)
    {
        _store = store;
        _conflits = conflits;
        _store.DonneesModifiees += (_, _) => Rafraichir();

        OuvrirCommand = new Command<SeanceAffichage>(async s =>
        {
            if (s is not null)
                await Shell.Current.GoToAsync($"seance?id={s.Seance.Id}");
        });

        ChargerElements();
    }

    public IReadOnlyList<string> Modes { get; } = new[] { "Par enseignant", "Par groupe", "Par salle" };

    public int ModeIndex
    {
        get => _modeIndex;
        set
        {
            if (SetProperty(ref _modeIndex, value))
                ChargerElements();
        }
    }

    public string LibelleChoix => ModeIndex switch
    {
        0 => "Choisir un enseignant",
        1 => "Choisir un groupe",
        _ => "Choisir une salle"
    };

    public ObservableCollection<ElementChoix> Elements { get; } = new();

    public ElementChoix? ElementChoisi
    {
        get => _element;
        set
        {
            if (SetProperty(ref _element, value))
                Rafraichir();
        }
    }

    public ObservableCollection<GroupeJour> SeancesParJour { get; } = new();

    public string Resume { get => _resume; private set => SetProperty(ref _resume, value); }

    public ICommand OuvrirCommand { get; }

    /// <summary>Titre utilisé pour l'export de la vue courante.</summary>
    public string TitreVue => ElementChoisi is null ? "Emploi du temps" : $"Emploi du temps — {ElementChoisi.Libelle}";

    /// <summary>Séances affichées dans la vue courante (utilisées par l'export).</summary>
    public IEnumerable<Seance> SeancesFiltrees()
    {
        if (ElementChoisi is null) return Enumerable.Empty<Seance>();
        var id = ElementChoisi.Id;

        return ModeIndex switch
        {
            0 => _store.Seances.Where(s => s.EnseignantId == id),
            1 => _store.Seances.Where(s => s.GroupeId == id),
            _ => _store.Seances.Where(s => s.SalleId == id)
        };
    }

    private void ChargerElements()
    {
        var idPrecedent = _element?.Id;

        var elements = ModeIndex switch
        {
            0 => _store.Enseignants.OrderBy(e => e.Nom).Select(e => new ElementChoix(e.Id, e.NomComplet)),
            1 => _store.Groupes.OrderBy(g => g.Nom).Select(g => new ElementChoix(g.Id, g.Nom)),
            _ => _store.Salles.OrderBy(s => s.Nom).Select(s => new ElementChoix(s.Id, s.Nom))
        };

        Elements.Clear();
        foreach (var e in elements)
            Elements.Add(e);

        OnPropertyChanged(nameof(LibelleChoix));
        ElementChoisi = Elements.FirstOrDefault(e => e.Id == idPrecedent) ?? Elements.FirstOrDefault();
        Rafraichir();
    }

    public void Rafraichir()
    {
        var enConflit = _conflits.SeancesEnConflit();
        var seances = SeancesFiltrees().ToList();

        SeancesParJour.Clear();
        var affichages = seances.Select(s =>
        {
            var a = _store.Decrire(s);
            a.EnConflit = enConflit.Contains(s.Id);
            return a;
        });
        foreach (var groupe in GroupeJour.Construire(affichages))
            SeancesParJour.Add(groupe);

        var heures = seances.Sum(s => (s.Fin - s.Debut).TotalHours);
        Resume = ElementChoisi is null
            ? "Aucun élément à afficher."
            : $"{seances.Count} séance(s) · {heures:0.#} h par semaine";

        OnPropertyChanged(nameof(TitreVue));
    }

    /// <summary>Appelé à l'affichage de la page pour tenir compte des ressources ajoutées entre-temps.</summary>
    public void Actualiser() => ChargerElements();
}
