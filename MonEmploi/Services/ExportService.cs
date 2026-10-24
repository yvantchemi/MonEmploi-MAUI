using System.Net;
using System.Text;
using MonEmploi.Models;

namespace MonEmploi.Services;

/// <summary>Export de l'emploi du temps en CSV (Excel) ou en HTML (impression / PDF), puis partage.</summary>
public class ExportService
{
    private readonly DataStore _store;

    public ExportService(DataStore store)
    {
        _store = store;
    }

    private IEnumerable<SeanceAffichage> Trier(IEnumerable<Seance> seances)
        => seances
            .OrderBy(s => Jours.Index(s.Jour))
            .ThenBy(s => s.Debut)
            .Select(_store.Decrire);

    /// <summary>Fichier CSV (séparateur point-virgule, UTF-8 avec BOM : s'ouvre directement dans Excel).</summary>
    public async Task<string> ExporterCsvAsync(string titre, IEnumerable<Seance> seances)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Jour;Début;Fin;Matière;Enseignant;Groupe;Salle");

        foreach (var s in Trier(seances))
        {
            sb.AppendLine(string.Join(";",
                s.Jour,
                s.Seance.Debut.ToString(@"hh\:mm"),
                s.Seance.Fin.ToString(@"hh\:mm"),
                Csv(s.Matiere), Csv(s.Enseignant), Csv(s.Groupe), Csv(s.Salle)));
        }

        var chemin = Path.Combine(FileSystem.CacheDirectory, NomFichier(titre, "csv"));
        await File.WriteAllTextAsync(chemin, sb.ToString(), new UTF8Encoding(true));
        await Partager(titre, chemin);
        return chemin;
    }

    /// <summary>Page HTML mise en forme, un tableau par jour, imprimable en PDF depuis le navigateur.</summary>
    public async Task<string> ExporterHtmlAsync(string titre, IEnumerable<Seance> seances)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"utf-8\">");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.Append($"<title>{H(titre)}</title><style>");
        sb.Append("body{font-family:Segoe UI,Roboto,Arial,sans-serif;margin:24px;color:#1B2430}");
        sb.Append("h1{color:#1F4E79;font-size:22px;margin-bottom:4px}p.sub{color:#5E6B7D;margin-top:0}");
        sb.Append("h2{color:#1F4E79;font-size:16px;margin:22px 0 6px}");
        sb.Append("table{border-collapse:collapse;width:100%}th,td{border:1px solid #E1E6EE;padding:6px 8px;text-align:left;font-size:13px}");
        sb.Append("th{background:#1F4E79;color:#fff}tr:nth-child(even) td{background:#F4F6FA}");
        sb.Append("</style></head><body>");
        sb.Append($"<h1>{H(titre)}</h1><p class=\"sub\">Généré par MonEmploi Éducation le {DateTime.Now:dd/MM/yyyy à HH:mm}</p>");

        var liste = Trier(seances).ToList();
        if (liste.Count == 0)
            sb.Append("<p>Aucune séance planifiée.</p>");

        foreach (var jour in liste.GroupBy(s => s.Jour))
        {
            sb.Append($"<h2>{H(jour.Key)}</h2><table><tr><th>Horaire</th><th>Matière</th><th>Enseignant</th><th>Groupe</th><th>Salle</th></tr>");
            foreach (var s in jour)
                sb.Append($"<tr><td>{H(s.Horaire)}</td><td>{H(s.Matiere)}</td><td>{H(s.Enseignant)}</td><td>{H(s.Groupe)}</td><td>{H(s.Salle)}</td></tr>");
            sb.Append("</table>");
        }

        sb.Append("</body></html>");

        var chemin = Path.Combine(FileSystem.CacheDirectory, NomFichier(titre, "html"));
        await File.WriteAllTextAsync(chemin, sb.ToString(), Encoding.UTF8);
        await Partager(titre, chemin);
        return chemin;
    }

    private static Task Partager(string titre, string chemin)
        => Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = titre,
            File = new ShareFile(chemin)
        });

    private static string NomFichier(string titre, string extension)
    {
        var propre = new string(titre.Select(c => c < 128 && char.IsLetterOrDigit(c) ? c : '_').ToArray()).Trim('_');
        while (propre.Contains("__")) propre = propre.Replace("__", "_");
        return $"{propre}.{extension}";
    }

    private static string H(string texte) => WebUtility.HtmlEncode(texte);

    private static string Csv(string valeur)
    {
        if (!valeur.Contains(';') && !valeur.Contains('"'))
            return valeur;

        var echappe = valeur.Replace("\"", "\"\"");
        return "\"" + echappe + "\"";
    }
}
