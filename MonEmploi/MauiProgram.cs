using MonEmploi.Services;
using MonEmploi.ViewModels;
using MonEmploi.Views;

namespace MonEmploi;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Services
        builder.Services.AddSingleton<DataStore>();

        // Ressources
        builder.Services.AddSingleton<SallesViewModel>();
        builder.Services.AddSingleton<SallesPage>();
        builder.Services.AddSingleton<EnseignantsViewModel>();
        builder.Services.AddSingleton<EnseignantsPage>();
        builder.Services.AddSingleton<GroupesViewModel>();
        builder.Services.AddSingleton<GroupesPage>();

        return builder.Build();
    }
}
