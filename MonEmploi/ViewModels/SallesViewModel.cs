using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class SallesViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private Salle? _selection;
    private string _nom = string.Empty;
    private string _batiment = string.Empty;
    private string _capacite = string.Empty;

    public SallesViewModel(DataStore store)
    {
        _store = store;
        EnregistrerCommand = new Command(async () => await Enregistrer());
        AnnulerCommand = new Command(Reinitialiser);
        SupprimerCommand = new Command<Salle>(async s => await Supprimer(s));
    }

    public ObservableCollection<Salle> Salles => _store.Salles;

    public string Nom { get => _nom; set => SetProperty(ref _nom, value); }
    public string Batiment { get => _batiment; set => SetProperty(ref _batiment, value); }
    public string Capacite { get => _capacite; set => SetProperty(ref _capacite, value); }

    public bool EnModification => _selection is not null;
    public string TexteBouton => EnModification ? "Mettre à jour" : "Ajouter la salle";

    public Salle? Selection
    {
        get => _selection;
        set
        {
            if (!SetProperty(ref _selection, value)) return;
            if (value is not null)
            {
                Nom = value.Nom;
                Batiment = value.Batiment;
                Capacite = value.Capacite.ToString();
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
            await AfficherMessage("Champ manquant", "Le nom de la salle est obligatoire.");
            return;
        }
        if (!int.TryParse(Capacite, out var capacite) || capacite <= 0)
        {
            await AfficherMessage("Capacité invalide", "Saisissez une capacité supérieure à zéro.");
            return;
        }
        if (Salles.Any(s => s.Nom.Equals(Nom.Trim(), StringComparison.OrdinalIgnoreCase) && s.Id != _selection?.Id))
        {
            await AfficherMessage("Doublon", "Une salle porte déjà ce nom.");
            return;
        }

        var salle = new Salle
        {
            Id = _selection?.Id ?? Guid.NewGuid().ToString("N"),
            Nom = Nom.Trim(),
            Batiment = Batiment.Trim(),
            Capacite = capacite
        };
        _store.EnregistrerSalle(salle);
        Reinitialiser();
    }

    private async Task Supprimer(Salle? salle)
    {
        if (salle is null) return;
        if (!await Confirmer("Supprimer", $"Supprimer la salle « {salle.Nom} » ?")) return;
        if (!_store.SupprimerSalle(salle))
            await AfficherMessage("Suppression impossible", "Cette salle est utilisée par au moins une séance.");
        else if (_selection?.Id == salle.Id)
            Reinitialiser();
    }

    private void Reinitialiser()
    {
        Selection = null;
        Nom = Batiment = Capacite = string.Empty;
    }
}
