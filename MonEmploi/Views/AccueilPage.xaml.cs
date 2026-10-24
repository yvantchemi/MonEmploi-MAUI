using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class AccueilPage : ContentPage
{
    private readonly AccueilViewModel _viewModel;

    public AccueilPage(AccueilViewModel viewModel)
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
