using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonEmploi.ViewModels;

/// <summary>Classe de base MVVM : notification des changements de propriétés.</summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? nom = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nom));

    protected bool SetProperty<T>(ref T champ, T valeur, [CallerMemberName] string? nom = null)
    {
        if (EqualityComparer<T>.Default.Equals(champ, valeur))
            return false;

        champ = valeur;
        OnPropertyChanged(nom);
        return true;
    }

    protected static Task AfficherMessage(string titre, string message)
        => Shell.Current?.DisplayAlert(titre, message, "OK") ?? Task.CompletedTask;

    protected static Task<bool> Confirmer(string titre, string message)
        => Shell.Current?.DisplayAlert(titre, message, "Oui", "Non") ?? Task.FromResult(false);
}
