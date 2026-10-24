using MonEmploi.Models;

namespace MonEmploi.Services;

public enum TypeConflit
{
    Salle,
    Enseignant,
    Groupe
}

/// <summary>Conflit détecté entre une séance candidate et une séance déjà planifiée.</summary>
public class Conflit
{
    public Conflit(TypeConflit type, Seance seanceExistante, string message)
    {
        Type = type;
        SeanceExistante = seanceExistante;
        Message = message;
    }

    public TypeConflit Type { get; }
    public Seance SeanceExistante { get; }
    public string Message { get; }

    public string Libelle => Type switch
    {
        TypeConflit.Salle => "Conflit de salle",
        TypeConflit.Enseignant => "Conflit d'enseignant",
        _ => "Conflit de groupe"
    };
}

/// <summary>
/// Règle métier : deux séances qui se chevauchent dans le temps ne peuvent partager
/// ni la même salle, ni le même enseignant, ni le même groupe.
/// </summary>
public class ConflictService
{
    private readonly DataStore _store;

    public ConflictService(DataStore store)
    {
        _store = store;
    }

    /// <summary>Conflits entre la séance candidate et toutes les séances enregistrées.</summary>
    public IReadOnlyList<Conflit> Detecter(Seance candidate) => Detecter(candidate, _store.Seances);

    public IReadOnlyList<Conflit> Detecter(Seance candidate, IEnumerable<Seance> existantes)
    {
        var conflits = new List<Conflit>();

        foreach (var autre in existantes)
        {
            // On ignore la séance elle-même (cas de la modification) et les créneaux qui ne se recoupent pas
            if (autre.Id == candidate.Id || !candidate.Chevauche(autre))
                continue;

            var quand = $"{Jours.Nom(autre.Jour)} {Jours.Horaire(autre.Debut, autre.Fin)}";

            if (!string.IsNullOrEmpty(candidate.SalleId) && autre.SalleId == candidate.SalleId)
            {
                var salle = _store.TrouverSalle(autre.SalleId)?.Nom ?? "La salle";
                conflits.Add(new Conflit(TypeConflit.Salle, autre,
                    $"{salle} est déjà occupée le {quand} ({autre.Matiere})."));
            }

            if (!string.IsNullOrEmpty(candidate.EnseignantId) && autre.EnseignantId == candidate.EnseignantId)
            {
                var enseignant = _store.TrouverEnseignant(autre.EnseignantId)?.NomComplet ?? "L'enseignant";
                conflits.Add(new Conflit(TypeConflit.Enseignant, autre,
                    $"{enseignant} donne déjà « {autre.Matiere} » le {quand}."));
            }

            if (!string.IsNullOrEmpty(candidate.GroupeId) && autre.GroupeId == candidate.GroupeId)
            {
                var groupe = _store.TrouverGroupe(autre.GroupeId)?.Nom ?? "Le groupe";
                conflits.Add(new Conflit(TypeConflit.Groupe, autre,
                    $"{groupe} a déjà « {autre.Matiere} » le {quand}."));
            }
        }

        return conflits;
    }

    /// <summary>
    /// Salles libres sur le créneau de la séance candidate et assez grandes pour l'effectif,
    /// de la plus petite à la plus grande (on évite de bloquer un amphi pour un petit groupe).
    /// </summary>
    public IReadOnlyList<Salle> SallesLibres(Seance candidate, int effectifMinimum)
        => _store.Salles
            .Where(salle => salle.Capacite >= effectifMinimum)
            .Where(salle => !_store.Seances.Any(s =>
                s.Id != candidate.Id && s.SalleId == salle.Id && s.Chevauche(candidate)))
            .OrderBy(salle => salle.Capacite)
            .ToList();

    /// <summary>Identifiants de toutes les séances enregistrées impliquées dans au moins un conflit.</summary>
    public HashSet<string> SeancesEnConflit()
    {
        var ids = new HashSet<string>();
        var seances = _store.Seances.ToList();

        for (var i = 0; i < seances.Count; i++)
        {
            for (var j = i + 1; j < seances.Count; j++)
            {
                var a = seances[i];
                var b = seances[j];
                if (!a.Chevauche(b)) continue;

                if (a.SalleId == b.SalleId || a.EnseignantId == b.EnseignantId || a.GroupeId == b.GroupeId)
                {
                    ids.Add(a.Id);
                    ids.Add(b.Id);
                }
            }
        }

        return ids;
    }
}
