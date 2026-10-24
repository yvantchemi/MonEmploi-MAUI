using System.Text.Json.Serialization;

namespace MonEmploi.Models;

/// <summary>Salle de cours (amphithéâtre, salle TD, laboratoire...).</summary>
public class Salle
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Nom { get; set; } = string.Empty;
    public string Batiment { get; set; } = string.Empty;
    public int Capacite { get; set; }

    [JsonIgnore]
    public string Details => string.IsNullOrWhiteSpace(Batiment)
        ? $"{Capacite} places"
        : $"{Batiment} · {Capacite} places";

    public override string ToString() => Nom;
}
