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

        SeancesParJour.Clear();
        foreach (var groupe in GroupeJour.Construire(affichages))
            SeancesParJour.Add(groupe);

        Resume = enConflit.Count == 0
            ? $"{affichages.Count} séance(s) planifiée(s) · aucun conflit"
            : $"{affichages.Count} séance(s) · {enConflit.Count} en conflit";
    }
}
