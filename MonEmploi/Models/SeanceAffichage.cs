namespace MonEmploi.Models;

/// <summary>Séance prête à l'affichage : les identifiants sont remplacés par les noms.</summary>
public class SeanceAffichage
{
    public SeanceAffichage(Seance seance, string enseignant, string groupe, string salle)
    {
        Seance = seance;
        Enseignant = enseignant;
        Groupe = groupe;
        Salle = salle;
    }

    public Seance Seance { get; }
    public string Enseignant { get; }
    public string Groupe { get; }
    public string Salle { get; }

    public string Matiere => Seance.Matiere;
    public string Jour => Jours.Nom(Seance.Jour);
    public string Horaire => Jours.Horaire(Seance.Debut, Seance.Fin);
    public string Participants => $"{Enseignant} · {Groupe}";
    public string Lieu => $"Salle : {Salle}";

    /// <summary>Vrai si la séance est en conflit avec une autre (salle, enseignant ou groupe).</summary>
    public bool EnConflit { get; set; }
}

/// <summary>Regroupement de séances par jour pour les CollectionView groupées.</summary>
public class GroupeJour : List<SeanceAffichage>
{
    public GroupeJour(string jour, IEnumerable<SeanceAffichage> seances) : base(seances)
    {
        Jour = jour;
    }

    public string Jour { get; }
    public string EnTete => $"{Jour} — {Count} séance(s)";

    public static List<GroupeJour> Construire(IEnumerable<SeanceAffichage> seances)
        => seances
            .OrderBy(s => Jours.Index(s.Seance.Jour))
            .ThenBy(s => s.Seance.Debut)
            .GroupBy(s => s.Seance.Jour)
            .Select(g => new GroupeJour(Jours.Nom(g.Key), g))
            .ToList();
}
