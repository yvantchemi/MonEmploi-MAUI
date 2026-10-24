using System.Text.Json.Serialization;

namespace MonEmploi.Models;

/// <summary>Enseignant intervenant dans l'établissement.</summary>
public class Enseignant
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Specialite { get; set; } = string.Empty;

    [JsonIgnore]
    public string NomComplet => string.IsNullOrWhiteSpace(Prenom) ? Nom : $"{Prenom} {Nom}";

    [JsonIgnore]
    public string Details => string.IsNullOrWhiteSpace(Specialite) ? "Spécialité non renseignée" : Specialite;

    public override string ToString() => NomComplet;
}
