using Analyzer.Models;

namespace Analyzer.Services
{
    /// <summary>
    /// Résumé chiffré d'une session (.ra1), indépendant de l'UI.
    /// </summary>
    public class SessionSummary
    {
        public string FilePath { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string? CircuitName { get; set; }
        public int CompleteLaps { get; set; }
        public int BestLapNumber { get; set; }
        public double BestLapMs { get; set; }
        public double IdealLapMs { get; set; }
        public double MaxSpeed { get; set; }
        public double MaxLeanLeft { get; set; }
        public double MaxLeanRight { get; set; }
        public double MaxDecelG { get; set; }
        public double MaxAccelG { get; set; }
        public double LapStdDevSeconds { get; set; }
        public int RepresentativeLaps { get; set; }
        public int CorruptedPoints { get; set; }
        /// <summary>Tours complets écartés car leur distance est anormale (ligne mal détectée, passage par les stands...).</summary>
        public int ExcludedLaps { get; set; }
        /// <summary>Faux pour les anciennes sessions où le boîtier n'enregistrait pas l'angle (et saturait l'accélération à ±2 G).</summary>
        public bool HasLeanData { get; set; }
        /// <summary>Temps des tours complets, dans l'ordre (ms).</summary>
        public List<double> LapTimesMs { get; set; } = new();
        /// <summary>Vmin de chaque virage sur le meilleur tour (données brutes, seuils de détection par défaut).</summary>
        public List<CornerComparison> BestLapCorners { get; set; } = new();
    }

    /// <summary>
    /// Enchaîne lecture .ra1 → détection du circuit → tours → statistiques, comme l'appli au chargement d'une session.
    /// </summary>
    public class SessionAnalyzer
    {
        private readonly Ra1ReaderService _reader = new();
        private readonly MapReaderService _mapReader = new();
        private readonly LapService _lapService = new();
        private readonly CornerService _cornerService = new();
        private readonly CircuitCatalog _catalog;

        public SessionAnalyzer(CircuitCatalog catalog) => _catalog = catalog;

        public SessionSummary Analyze(string filePath, CornerDetectionSettings? cornerSettings = null)
        {
            var summary = new SessionSummary
            {
                FilePath = filePath,
                FolderName = Path.GetFileName(Path.GetDirectoryName(filePath)) ?? "",
                Date = TimeFormat.ParseSessionDate(Path.GetFileNameWithoutExtension(filePath))
                       ?? TimeFormat.ParseSessionDate(Path.GetFileName(Path.GetDirectoryName(filePath)) ?? "")
            };

            var points = _reader.ReadFile(filePath);
            summary.CorruptedPoints = _reader.LastCorruptedPointsCount;
            if (points.Count == 0) return summary;
            summary.HasLeanData = points.Any(p => Math.Abs(p.LeanAngle) > 0.5f);

            var circuit = _catalog.Detect(points) ?? _catalog.FindByFolderName(filePath);
            if (circuit == null) return summary;
            summary.CircuitName = circuit.Name;

            var map = _mapReader.ReadMap(circuit.FilePath);
            var laps = _lapService.CalculateLaps(points, map);
            if (laps.Count == 0) return summary;

            summary.MaxSpeed = laps.Max(l => l.MaxSpeed);
            summary.MaxLeanLeft = laps.Max(l => l.MaxLeanLeft);
            summary.MaxLeanRight = laps.Max(l => l.MaxLeanRight);
            summary.MaxDecelG = laps.Max(l => l.MaxDecel);
            summary.MaxAccelG = laps.Max(l => l.MaxAccel);

            var complete = laps.Where(l => l.Type == "Complet" && l.LapTimeMs > 0).ToList();
            complete = ExcludeDistanceAnomalies(complete, points, out int excluded);
            summary.ExcludedLaps = excluded;
            summary.CompleteLaps = complete.Count;
            summary.LapTimesMs = complete.Select(l => l.LapTimeMs).ToList();
            if (complete.Count == 0) return summary;

            var best = complete.OrderBy(l => l.LapTimeMs).First();
            summary.BestLapNumber = best.Number;
            summary.BestLapMs = best.LapTimeMs;
            summary.IdealLapMs = LapStatistics.IdealLapMs(complete, LapStatistics.SectorCount(map));
            summary.LapStdDevSeconds = LapStatistics.LapTimeStdDevSeconds(complete, out int used);
            summary.RepresentativeLaps = used;

            best.TelemetryPoints = points
                .Where(p => p.Time >= best.StartTimeMs && p.Time <= best.StartTimeMs + best.LapTimeMs)
                .ToList();
            var corners = _cornerService.DetectCorners(best.TelemetryPoints, cornerSettings ?? new CornerDetectionSettings());
            summary.BestLapCorners = _cornerService.CompareLaps(null, best, corners);

            return summary;
        }

        /// <summary>
        /// Écarte les tours dont la distance parcourue s'écarte de plus de 10 % de la médiane de la session.
        /// </summary>
        private static List<LapData> ExcludeDistanceAnomalies(List<LapData> complete, List<TelemetryPoint> points, out int excluded)
        {
            excluded = 0;
            if (complete.Count < 3) return complete;

            var distances = complete.ToDictionary(l => l, l =>
            {
                double end = l.StartTimeMs + l.LapTimeMs;
                var last = points.LastOrDefault(p => p.Time <= end);
                return last == null ? 0 : last.Distance - l.StartDistance;
            });

            var sorted = distances.Values.OrderBy(d => d).ToList();
            double median = sorted[sorted.Count / 2];
            if (median <= 0) return complete;

            var kept = complete.Where(l => Math.Abs(distances[l] - median) <= median * 0.10).ToList();
            excluded = complete.Count - kept.Count;
            return kept;
        }
    }
}
