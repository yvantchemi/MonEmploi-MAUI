using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SeancesViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private string _resume = string.Empty;

    public SeancesViewModel(DataStore store)
    {
        _store = store;
        _store.DonneesModifiees += (_, _) => Rafraichir();

        NouvelleCommand = new Command(async () => await Shell.Current.GoToAsync("seance"));

        Rafraichir();
    }

    public ObservableCollection<GroupeJour> SeancesParJour { get; } = new();

    public string Resume { get => _resume; private set => SetProperty(ref _resume, value); }

    public ICommand NouvelleCommand { get; }

    public void Rafraichir()
    {
        var affichages = _store.Seances.Select(_store.Decrire).ToList();

        SeancesParJour.Clear();
        foreach (var groupe in GroupeJour.Construire(affichages))
            SeancesParJour.Add(groupe);

        Resume = $"{affichages.Count} séance(s) planifiée(s)";
    }
}
