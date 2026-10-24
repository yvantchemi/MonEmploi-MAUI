using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class SallesPage : ContentPage
{
    public SallesPage(SallesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
