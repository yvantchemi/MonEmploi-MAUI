using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class SeanceEditPage : ContentPage
{
    public SeanceEditPage(SeanceEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
