using System.Collections.ObjectModel;
using System.Windows.Input;
using MonEmploi.Models;
using MonEmploi.Services;

namespace MonEmploi.ViewModels;

public class EnseignantsViewModel : BaseViewModel
{
    private readonly DataStore _store;
    private Enseignant? _selection;
    private string _nom = string.Empty;
    private string _prenom = string.Empty;
    private string _specialite = string.Empty;

    public EnseignantsViewModel(DataStore store)
    {
        _store = store;
        EnregistrerCommand = new Command(async () => await Enregistrer());
        AnnulerCommand = new Command(Reinitialiser);
        SupprimerCommand = new Command<Enseignant>(async e => await Supprimer(e));
    }

    public ObservableCollection<Enseignant> Enseignants => _store.Enseignants;

    public string Nom { get => _nom; set => SetProperty(ref _nom, value); }
    public string Prenom { get => _prenom; set => SetProperty(ref _prenom, value); }
    public string Specialite { get => _specialite; set => SetProperty(ref _specialite, value); }

    public bool EnModification => _selection is not null;
    public string TexteBouton => EnModification ? "Mettre à jour" : "Ajouter l'enseignant";

    public Enseignant? Selection
    {
        get => _selection;
        set
        {
            if (!SetProperty(ref _selection, value)) return;
            if (value is not null)
            {
                Nom = value.Nom;
                Prenom = value.Prenom;
                Specialite = value.Specialite;
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
            await AfficherMessage("Champ manquant", "Le nom de l'enseignant est obligatoire.");
            return;
        }

        var enseignant = new Enseignant
        {
            Id = _selection?.Id ?? Guid.NewGuid().ToString("N"),
            Nom = Nom.Trim().ToUpperInvariant(),
            Prenom = Prenom.Trim(),
            Specialite = Specialite.Trim()
        };
        _store.EnregistrerEnseignant(enseignant);
        Reinitialiser();
    }

    private async Task Supprimer(Enseignant? enseignant)
    {
        if (enseignant is null) return;
        if (!await Confirmer("Supprimer", $"Supprimer « {enseignant.NomComplet} » ?")) return;
        if (!_store.SupprimerEnseignant(enseignant))
            await AfficherMessage("Suppression impossible", "Cet enseignant a encore des séances planifiées.");
        else if (_selection?.Id == enseignant.Id)
            Reinitialiser();
    }

    private void Reinitialiser()
    {
        Selection = null;
        Nom = Prenom = Specialite = string.Empty;
    }
}
