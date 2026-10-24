using MonEmploi.ViewModels;

namespace MonEmploi.Views;

public partial class GroupesPage : ContentPage
{
    public GroupesPage(GroupesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
