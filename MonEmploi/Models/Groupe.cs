using System.Text.Json.Serialization;

namespace MonEmploi.Models;

/// <summary>Groupe d'étudiants (classe, promotion, groupe de TD).</summary>
public class Groupe
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nom { get; set; } = string.Empty;
    public string Filiere { get; set; } = string.Empty;
    public int Effectif { get; set; }

    [JsonIgnore]
    public string Details => string.IsNullOrWhiteSpace(Filiere)
        ? $"{Effectif} étudiants"
        : $"{Filiere} · {Effectif} étudiants";

    public override string ToString() => Nom;
}
