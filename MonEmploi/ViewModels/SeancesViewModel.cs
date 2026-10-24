using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SeancesViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private readonly ConflictService _conflits;
    private string _resume = string.Empty;

    public SeancesViewModel(DataStore store, ConflictService conflits)
    {
        _store = store;
        _conflits = conflits;
        _store.DonneesModifiees += (_, _) => Rafraichir();

        NouvelleCommand = new Command(async () => await Shell.Current.GoToAsync("seance"));
        OuvrirCommand = new Command<SeanceAffichage>(async s =>
        {
            if (s is not null)
                await Shell.Current.GoToAsync($"seance?id={s.Seance.Id}");
        });

        Rafraichir();
    }

    public ObservableCollection<GroupeJour> SeancesParJour { get; } = new();

    public string Resume { get => _resume; private set => SetProperty(ref _resume, value); }

    private string _recherche = string.Empty;
    private bool _conflitsSeulement;

    /// <summary>Filtre texte sur la matière, l'enseignant, le groupe ou la salle.</summary>
    public string Recherche
    {
        get => _recherche;
        set { if (SetProperty(ref _recherche, value)) Rafraichir(); }
    }

    /// <summary>N'afficher que les séances en conflit.</summary>
    public bool ConflitsSeulement
    {
        get => _conflitsSeulement;
        set { if (SetProperty(ref _conflitsSeulement, value)) Rafraichir(); }
    }

    private bool Correspond(SeanceAffichage s)
    {
        if (ConflitsSeulement && !s.EnConflit) return false;
        if (string.IsNullOrWhiteSpace(Recherche)) return true;

        var terme = Recherche.Trim();
        return new[] { s.Matiere, s.Enseignant, s.Groupe, s.Salle }
            .Any(champ => champ.Contains(terme, StringComparison.OrdinalIgnoreCase));
    }

    public ICommand NouvelleCommand { get; }
    public ICommand OuvrirCommand { get; }

    public void Rafraichir()
    {
        var enConflit = _conflits.SeancesEnConflit();
        var affichages = _store.Seances.Select(s =>
        {
            var a = _store.Decrire(s);
            a.EnConflit = enConflit.Contains(s.Id);
            return a;
        }).ToList();

        var visibles = affichages.Where(Correspond).ToList();

        SeancesParJour.Clear();
        foreach (var groupe in GroupeJour.Construire(visibles))
            SeancesParJour.Add(groupe);

        var filtre = visibles.Count == affichages.Count ? string.Empty : $" · {visibles.Count} affichée(s)";
        Resume = enConflit.Count == 0
            ? $"{affichages.Count} séance(s) · aucun conflit{filtre}"
            : $"{affichages.Count} séance(s) · {enConflit.Count} en conflit{filtre}";
    }
}
