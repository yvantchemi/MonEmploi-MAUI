using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class SeancesPage : ContentPage
{
    private readonly SeancesViewModel _viewModel;

    public SeancesPage(SeancesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Rafraichir();
    }
}
