using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class GroupesViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private Groupe? _selection;
    private string _nom = string.Empty;
    private string _filiere = string.Empty;
    private string _effectif = string.Empty;

    public GroupesViewModel(DataStore store)
    {
        _store = store;
        EnregistrerCommand = new Command(async () => await Enregistrer());
        AnnulerCommand = new Command(Reinitialiser);
        SupprimerCommand = new Command<Groupe>(async g => await Supprimer(g));
    }

    public ObservableCollection<Groupe> Groupes => _store.Groupes;

    public string Nom { get => _nom; set => SetProperty(ref _nom, value); }
    public string Filiere { get => _filiere; set => SetProperty(ref _filiere, value); }
    public string Effectif { get => _effectif; set => SetProperty(ref _effectif, value); }

    public bool EnModification => _selection is not null;
    public string TexteBouton => EnModification ? "Mettre à jour" : "Ajouter le groupe";

    public Groupe? Selection
    {
        get => _selection;
        set
        {
            if (!SetProperty(ref _selection, value)) return;
            if (value is not null)
            {
                Nom = value.Nom;
                Filiere = value.Filiere;
                Effectif = value.Effectif.ToString();
            }
            OnPropertyChanged(nameof(EnModification));
            OnPropertyChanged(nameof(TexteBouton));
        }
    }

    public ICommand EnregistrerCommand { get; }
    public ICommand AnnulerCommand { get; }
    public ICommand SupprimerCommand { get; }

    private async Task Enregistrer()
    {
        if (string.IsNullOrWhiteSpace(Nom))
        {
            await AfficherMessage("Champ manquant", "Le nom du groupe est obligatoire.");
            return;
        }
        if (!int.TryParse(Effectif, out var effectif) || effectif <= 0)
        {
            await AfficherMessage("Effectif invalide", "Saisissez un effectif supérieur à zéro.");
            return;
        }

        var groupe = new Groupe
        {
            Id = _selection?.Id ?? Guid.NewGuid().ToString("N"),
            Nom = Nom.Trim(),
            Filiere = Filiere.Trim(),
            Effectif = effectif
        };
        _store.EnregistrerGroupe(groupe);
        Reinitialiser();
    }

    private async Task Supprimer(Groupe? groupe)
    {
        if (groupe is null) return;
        if (!await Confirmer("Supprimer", $"Supprimer le groupe « {groupe.Nom} » ?")) return;
        if (!_store.SupprimerGroupe(groupe))
            await AfficherMessage("Suppression impossible", "Ce groupe a encore des séances planifiées.");
        else if (_selection?.Id == groupe.Id)
            Reinitialiser();
    }

    private void Reinitialiser()
    {
        Selection = null;
        Nom = Filiere = Effectif = string.Empty;
    }
}
