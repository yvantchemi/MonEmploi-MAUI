using MonEmploi.Views;

namespace MonEmploi;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages ouvertes par navigation (hors barre d'onglets)
        Routing.RegisterRoute("seance", typeof(SeanceEditPage));
    }
}
