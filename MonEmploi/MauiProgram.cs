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
        builder.Services.AddSingleton<ConflictService>();

        // Ressources
        builder.Services.AddSingleton<SallesViewModel>();
        builder.Services.AddSingleton<SallesPage>();
        builder.Services.AddSingleton<EnseignantsViewModel>();
        builder.Services.AddSingleton<EnseignantsPage>();
        builder.Services.AddSingleton<GroupesViewModel>();
        builder.Services.AddSingleton<GroupesPage>();

        // Séances
        builder.Services.AddSingleton<SeancesViewModel>();
        builder.Services.AddSingleton<SeancesPage>();
        builder.Services.AddTransient<SeanceEditViewModel>();
        builder.Services.AddTransient<SeanceEditPage>();

        // Vues par enseignant / groupe / salle
        builder.Services.AddSingleton<PlanningViewModel>();
        builder.Services.AddSingleton<PlanningPage>();

        return builder.Build();
    }
}
