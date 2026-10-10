using System.Globalization;
using System.Text;
using Analyzer.Services;

// Calcule les mesures par circuit à partir des sessions .ra1 et met à jour la section
// automatique d'une fiche pilote. Usage : voir PrintUsage().

const string BeginMarker = "<!-- AUTO:MESURES:DEBUT - section générée par Tools/ProfileStats, ne pas modifier à la main -->";
const string EndMarker = "<!-- AUTO:MESURES:FIN -->";

var dataFolders = new List<string>();
string? profilePath = null;
string? mapsRoot = null;
bool printOnly = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--data" when i + 1 < args.Length: dataFolders.Add(args[++i]); break;
        case "--profile" when i + 1 < args.Length: profilePath = args[++i]; break;
        case "--maps" when i + 1 < args.Length: mapsRoot = args[++i]; break;
        case "--print": printOnly = true; break;
        case "-h" or "--help": PrintUsage(); return 0;
        default:
            Console.Error.WriteLine($"Argument inconnu : {args[i]}");
            PrintUsage();
            return 1;
    }
}

if (dataFolders.Count == 0 || (profilePath == null && !printOnly))
{
    PrintUsage();
    return 1;
}

mapsRoot ??= FindMapsRoot();
if (mapsRoot == null || !Directory.Exists(mapsRoot))
{
    Console.Error.WriteLine("Bibliothèque de circuits introuvable : préciser --maps <dossier Map/Circuits>.");
    return 1;
}

var catalog = new CircuitCatalog();
catalog.Load(mapsRoot);
var analyzer = new SessionAnalyzer(catalog);

// Sessions : un fichier présent dans plusieurs dossiers de données (copie de test) n'est compté qu'une fois
var files = dataFolders
    .Where(Directory.Exists)
    .SelectMany(d => Directory.GetFiles(d, "*.ra1", SearchOption.AllDirectories))
    .GroupBy(f => (Path.GetFileName(Path.GetDirectoryName(f)) ?? "") + "/" + Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
    .Select(g => g.First())
    .ToList();

Console.Error.WriteLine($"{files.Count} session(s) trouvée(s), analyse en cours...");
var summaries = new List<SessionSummary>();
foreach (var file in files)
{
    try { summaries.Add(analyzer.Analyze(file)); }
    catch (Exception ex) { Console.Error.WriteLine($"  ! {file} : {ex.Message}"); }
}

string section = BuildSection(summaries, dataFolders);

if (printOnly)
{
    Console.WriteLine(section);
    return 0;
}

var profile = File.Exists(profilePath!) ? File.ReadAllText(profilePath!) : "";
File.WriteAllText(profilePath!, ReplaceSection(profile, section), new UTF8Encoding(false));
Console.Error.WriteLine($"Section « Mesures » mise à jour dans {profilePath}");
return 0;

// ---------------------------------------------------------------------------------------------

static string BuildSection(List<SessionSummary> summaries, List<string> dataFolders)
{
    var fr = CultureInfo.GetCultureInfo("fr-FR");
    var sb = new StringBuilder();
    var analysed = summaries.Where(s => s.CircuitName != null).ToList();

    sb.AppendLine(BeginMarker);
    sb.AppendLine("## Mesures (automatique)");
    sb.AppendLine();
    sb.AppendLine($"_Mis à jour le {DateTime.Now:dd/MM/yyyy} à partir de {analysed.Count} session(s). Temps et tours calculés par l'analyseur (détection de la ligne Start du .map) ; Vmin sur les données brutes du meilleur tour, seuils de détection par défaut._");
    sb.AppendLine();

    if (analysed.Any(s => !s.HasLeanData))
        sb.AppendLine("_n/d : sessions anciennes où le boîtier n'enregistrait pas l'angle (accélération saturée à ±2 G) ; vitesses et temps restent fiables._\n");
    if (analysed.Any(s => s.ExcludedLaps > 0))
        sb.AppendLine("_Tours écartés : distance anormale (> 10 % d'écart avec la médiane de la session), exclus du meilleur tour, du tour idéal et de la régularité._\n");

    var unknown = summaries.Count - analysed.Count;
    if (unknown > 0) sb.AppendLine($"_{unknown} session(s) ignorée(s) : circuit non reconnu ou fichier vide._\n");

    foreach (var circuit in analysed.GroupBy(s => s.CircuitName!).OrderByDescending(g => g.Max(s => s.Date ?? DateTime.MinValue)))
    {
        var withLaps = circuit.Where(s => s.BestLapMs > 0).ToList();
        var record = withLaps.OrderBy(s => s.BestLapMs).FirstOrDefault();
        int days = circuit.Select(s => s.Date?.Date ?? DateTime.MinValue).Distinct().Count();

        sb.Append($"### {circuit.Key} - {days} journée(s), {circuit.Count()} session(s)");
        if (record != null) sb.Append($" - record {Lap(record.BestLapMs)} ({When(record)}, tour {record.BestLapNumber})");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("| Session | Tours | Meilleur | Idéal | Régularité | Vmax | Angle max G / D | Frein max |");
        sb.AppendLine("| :--- | ---: | ---: | ---: | :--- | ---: | ---: | ---: |");
        foreach (var s in circuit.OrderBy(s => s.Date ?? DateTime.MinValue))
        {
            string regularity = s.LapStdDevSeconds > 0
                ? string.Format(fr, "± {0:0.00} s ({1} tours)", s.LapStdDevSeconds, s.RepresentativeLaps)
                : "-";
            string laps = s.ExcludedLaps > 0 ? $"{s.CompleteLaps} (+{s.ExcludedLaps} écarté)" : s.CompleteLaps.ToString();
            string lean = s.HasLeanData ? string.Format(fr, "{0:0}° / {1:0}°", s.MaxLeanLeft, s.MaxLeanRight) : "n/d";
            string brake = s.HasLeanData ? string.Format(fr, "{0:0.00} G", s.MaxDecelG) : "n/d";
            if (s.MaxSpeed <= 0)
            {
                sb.AppendLine($"| {When(s)} | 0 | - | - | - | - | - | - |");
                continue;
            }
            sb.AppendLine(string.Format(fr, "| {0} | {1} | {2} | {3} | {4} | {5:0} km/h | {6} | {7} |",
                When(s), laps, s.BestLapMs > 0 ? Lap(s.BestLapMs) : "-", s.IdealLapMs > 0 ? Lap(s.IdealLapMs) : "-",
                regularity, s.MaxSpeed, lean, brake));
        }

        if (record != null && record.BestLapCorners.Count > 0)
        {
            sb.AppendLine();
            var vmins = record.BestLapCorners.Select(c => string.Format(fr, "V{0} {1:0}", c.Number, c.SelectedVmin));
            sb.AppendLine($"Vmin par virage sur le record (km/h) : {string.Join(" · ", vmins)}");
        }
        else if (record != null && !record.HasLeanData)
        {
            sb.AppendLine();
            sb.AppendLine("Vmin par virage : n/d (angle non enregistré sur la session du record).");
        }
        sb.AppendLine();
    }

    sb.Append(EndMarker);
    return sb.ToString();
}

static string ReplaceSection(string profile, string section)
{
    string nl = profile.Contains("\r\n") ? "\r\n" : "\n";
    section = section.Replace("\r\n", "\n").Replace("\n", nl);

    int begin = profile.IndexOf(BeginMarker, StringComparison.Ordinal);
    int end = profile.IndexOf(EndMarker, StringComparison.Ordinal);
    if (begin >= 0 && end > begin)
        return profile[..begin] + section + profile[(end + EndMarker.Length)..];

    return profile.TrimEnd() + nl + nl + section + nl;
}

// 66650 ms → 1'06.65 (format des fiches pilotes)
static string Lap(double ms)
{
    var t = TimeSpan.FromMilliseconds(ms);
    return $"{(int)t.TotalMinutes}'{t.Seconds:D2}.{t.Milliseconds / 10:D2}";
}

static string When(SessionSummary s) =>
    s.Date is DateTime d ? (d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-dd HH'h'mm")) : Path.GetFileNameWithoutExtension(s.FilePath);

static string? FindMapsRoot()
{
    // Remonte depuis l'exécutable puis le dossier courant jusqu'à trouver Map/Circuits (racine du repo)
    foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
    {
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "Map", "Circuits");
            if (Directory.Exists(candidate)) return candidate;
        }
    }
    return null;
}

static void PrintUsage()
{
    Console.Error.WriteLine("""
        Mesures du profil pilote à partir des sessions 3DMS (.ra1).

        Usage :
          3dms-profile --data <dossier> [--data <dossier> ...] --profile <fiche.md> [--maps <Map/Circuits>]
          3dms-profile --data <dossier> --print

          --data     Dossier de sessions (sous-dossiers parcourus). Répétable ; doublons ignorés.
          --profile  Fiche pilote à mettre à jour (section entre balises AUTO:MESURES, le reste n'est pas touché).
          --maps     Bibliothèque de circuits (par défaut : Map/Circuits trouvé en remontant les dossiers).
          --print    Affiche la section sans écrire la fiche.
        """);
}
