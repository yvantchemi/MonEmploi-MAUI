namespace MonEmploi.Models;

/// <summary>Jours ouvrables de l'établissement (lundi à samedi).</summary>
public static class Jours
{
    public static readonly IReadOnlyList<string> Noms =
        new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi" };

    public static string Nom(DayOfWeek jour)
        => jour == DayOfWeek.Sunday ? "Dimanche" : Noms[(int)jour - 1];

    /// <summary>Index dans la liste <see cref="Noms"/> (0 = lundi).</summary>
    public static int Index(DayOfWeek jour) => jour == DayOfWeek.Sunday ? 0 : (int)jour - 1;

    public static DayOfWeek DepuisIndex(int index) => (DayOfWeek)(Math.Clamp(index, 0, 5) + 1);

    public static string Horaire(TimeSpan debut, TimeSpan fin)
        => $"{debut:hh\\:mm} – {fin:hh\\:mm}";
}
