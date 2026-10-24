using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class PlanningPage : ContentPage
{
    private readonly PlanningViewModel _viewModel;

    public PlanningPage(PlanningViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Actualiser();
    }
}
