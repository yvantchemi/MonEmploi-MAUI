using System.Collections.ObjectModel;
using MonEmploi.Models;

namespace MonEmploi.Services;

/// <summary>
/// Source unique des données de l'application : salles, enseignants, groupes et séances.
/// </summary>
public class DataStore
{
    public ObservableCollection<Salle> Salles { get; } = new();
    public ObservableCollection<Enseignant> Enseignants { get; } = new();
    public ObservableCollection<Groupe> Groupes { get; } = new();
    public ObservableCollection<Seance> Seances { get; } = new();

    /// <summary>Déclenché après chaque modification des données.</summary>
    public event EventHandler? DonneesModifiees;

    public DataStore()
    {
        ChargerDonneesDemo();
    }

    // ---------- Recherche ----------
    public Salle? TrouverSalle(string? id) => Salles.FirstOrDefault(s => s.Id == id);
    public Enseignant? TrouverEnseignant(string? id) => Enseignants.FirstOrDefault(e => e.Id == id);
    public Groupe? TrouverGroupe(string? id) => Groupes.FirstOrDefault(g => g.Id == id);
    public Seance? TrouverSeance(string? id) => Seances.FirstOrDefault(s => s.Id == id);

    // ---------- Salles ----------
    public void EnregistrerSalle(Salle salle) => Remplacer(Salles, salle, s => s.Id == salle.Id);

    public bool SupprimerSalle(Salle salle)
    {
        if (Seances.Any(s => s.SalleId == salle.Id)) return false;
        Salles.Remove(salle);
        Notifier();
        return true;
    }

    // ---------- Enseignants ----------
    public void EnregistrerEnseignant(Enseignant enseignant) => Remplacer(Enseignants, enseignant, e => e.Id == enseignant.Id);

    public bool SupprimerEnseignant(Enseignant enseignant)
    {
        if (Seances.Any(s => s.EnseignantId == enseignant.Id)) return false;
        Enseignants.Remove(enseignant);
        Notifier();
        return true;
    }

    // ---------- Groupes ----------
    public void EnregistrerGroupe(Groupe groupe) => Remplacer(Groupes, groupe, g => g.Id == groupe.Id);

    public bool SupprimerGroupe(Groupe groupe)
    {
        if (Seances.Any(s => s.GroupeId == groupe.Id)) return false;
        Groupes.Remove(groupe);
        Notifier();
        return true;
    }

    // ---------- Séances ----------
    public void EnregistrerSeance(Seance seance) => Remplacer(Seances, seance, s => s.Id == seance.Id);

    public void SupprimerSeance(Seance seance)
    {
        var existante = TrouverSeance(seance.Id);
        if (existante is null) return;
        Seances.Remove(existante);
        Notifier();
    }

    // ---------- Interne ----------
    private void Remplacer<T>(ObservableCollection<T> liste, T element, Func<T, bool> memeId)
    {
        var existant = liste.FirstOrDefault(memeId);
        if (existant is null)
            liste.Add(element);
        else
            liste[liste.IndexOf(existant)] = element;

        Notifier();
    }

    private void Notifier() => DonneesModifiees?.Invoke(this, EventArgs.Empty);

    private void ChargerDonneesDemo()
    {
        Salles.Clear(); Enseignants.Clear(); Groupes.Clear(); Seances.Clear();

        var amphi = new Salle { Nom = "Amphi A", Batiment = "Bâtiment principal", Capacite = 120 };
        var s101 = new Salle { Nom = "Salle 101", Batiment = "Bloc B", Capacite = 40 };
        var s102 = new Salle { Nom = "Salle 102", Batiment = "Bloc B", Capacite = 30 };
        var labo = new Salle { Nom = "Labo Info 1", Batiment = "Bloc C", Capacite = 25 };
        foreach (var s in new[] { amphi, s101, s102, labo }) Salles.Add(s);

        var mbarga = new Enseignant { Prenom = "Paul", Nom = "MBARGA", Specialite = "Mathématiques" };
        var ngo = new Enseignant { Prenom = "Claire", Nom = "NGO BIYONG", Specialite = "Développement mobile" };
        var fotso = new Enseignant { Prenom = "Jean", Nom = "FOTSO", Specialite = "Réseaux" };
        var atangana = new Enseignant { Prenom = "Marie", Nom = "ATANGANA", Specialite = "Bases de données" };
        foreach (var e in new[] { mbarga, ngo, fotso, atangana }) Enseignants.Add(e);

        var l3 = new Groupe { Nom = "L3 Génie Logiciel", Filiere = "Informatique", Effectif = 38 };
        var l2 = new Groupe { Nom = "L2 Réseaux", Filiere = "Télécoms", Effectif = 28 };
        var m1 = new Groupe { Nom = "M1 Data", Filiere = "Informatique", Effectif = 22 };
        foreach (var g in new[] { l3, l2, m1 }) Groupes.Add(g);

        Seances.Add(new Seance { Matiere = "Analyse numérique", EnseignantId = mbarga.Id, GroupeId = l3.Id, SalleId = s101.Id, Jour = DayOfWeek.Monday, Debut = new(8, 0, 0), Fin = new(10, 0, 0) });
        Seances.Add(new Seance { Matiere = "Développement .NET MAUI", EnseignantId = ngo.Id, GroupeId = l3.Id, SalleId = labo.Id, Jour = DayOfWeek.Monday, Debut = new(10, 30, 0), Fin = new(12, 30, 0) });
        Seances.Add(new Seance { Matiere = "Routage et commutation", EnseignantId = fotso.Id, GroupeId = l2.Id, SalleId = s102.Id, Jour = DayOfWeek.Monday, Debut = new(8, 0, 0), Fin = new(11, 0, 0) });
        Seances.Add(new Seance { Matiere = "SQL avancé", EnseignantId = atangana.Id, GroupeId = m1.Id, SalleId = labo.Id, Jour = DayOfWeek.Tuesday, Debut = new(9, 0, 0), Fin = new(12, 0, 0) });
        Seances.Add(new Seance { Matiere = "Probabilités", EnseignantId = mbarga.Id, GroupeId = l2.Id, SalleId = amphi.Id, Jour = DayOfWeek.Wednesday, Debut = new(14, 0, 0), Fin = new(16, 0, 0) });
        Seances.Add(new Seance { Matiere = "Architecture mobile", EnseignantId = ngo.Id, GroupeId = m1.Id, SalleId = s101.Id, Jour = DayOfWeek.Thursday, Debut = new(8, 0, 0), Fin = new(10, 0, 0) });
    }
}
