using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class EnseignantsPage : ContentPage
{
    public EnseignantsPage(EnseignantsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
