namespace MonEmploi.Models;

/// <summary>Conteneur sérialisé dans le fichier local monemploi.json.</summary>
public class DonneesEmploi
{
    public List<Salle> Salles { get; set; } = new();
    public List<Enseignant> Enseignants { get; set; } = new();
    public List<Groupe> Groupes { get; set; } = new();
    public List<Seance> Seances { get; set; } = new();
}
