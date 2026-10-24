namespace MonEmploi.Models;

/// <summary>Séance de cours : une matière, un enseignant, un groupe, une salle, un créneau.</summary>
public class Seance
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Matiere { get; set; } = string.Empty;
    public string EnseignantId { get; set; } = string.Empty;
    public string GroupeId { get; set; } = string.Empty;
    public string SalleId { get; set; } = string.Empty;
    public DayOfWeek Jour { get; set; } = DayOfWeek.Monday;
    public TimeSpan Debut { get; set; } = new(8, 0, 0);
    public TimeSpan Fin { get; set; } = new(10, 0, 0);

    public Seance Cloner() => (Seance)MemberwiseClone();

    /// <summary>Deux séances se chevauchent si elles ont lieu le même jour et que leurs créneaux se recoupent.</summary>
    public bool Chevauche(Seance autre)
        => Jour == autre.Jour && Debut < autre.Fin && autre.Debut < Fin;
}
