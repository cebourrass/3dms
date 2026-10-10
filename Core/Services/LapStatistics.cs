using Analyzer.Models;

namespace Analyzer.Services
{
    /// <summary>
    /// Statistiques calculées sur les tours d'une session.
    /// </summary>
    public static class LapStatistics
    {
        /// <summary>
        /// Nombre de secteurs d'un circuit : un de plus que le nombre de marqueurs "Time*" (0 si aucun).
        /// </summary>
        public static int SectorCount(TrackMap map)
        {
            int markerTimes = map.Markers.Keys.Count(k => k.StartsWith("Time", StringComparison.OrdinalIgnoreCase));
            return markerTimes > 0 ? markerTimes + 1 : 0;
        }

        /// <summary>
        /// Tour idéal : somme des meilleurs temps de chaque secteur sur les tours complets. 0 si non calculable.
        /// </summary>
        public static double IdealLapMs(IEnumerable<LapData> completeLaps, int sectorCount)
        {
            var laps = completeLaps.ToList();
            if (sectorCount <= 0 || laps.Count == 0) return 0;

            double idealMs = 0;
            for (int s = 0; s < sectorCount; s++)
            {
                idealMs += laps
                    .Select(l => s < l.Partials.Length ? TimeFormat.ParseToMs(l.Partials[s]) : 0)
                    .Where(ms => ms > 0)
                    .DefaultIfEmpty(0)
                    .Min();
            }
            return idealMs;
        }

        /// <summary>
        /// Écart-type (s) des temps des tours "représentatifs" : tours complets à moins de 107 % du meilleur,
        /// pour écarter les tours de rentrée / trafic.
        /// </summary>
        public static double LapTimeStdDevSeconds(IEnumerable<LapData> completeLaps, out int lapsUsed)
        {
            var times = completeLaps.Select(l => l.LapTimeMs).Where(t => t > 0).ToList();
            lapsUsed = 0;
            if (times.Count < 2) return 0;

            double best = times.Min();
            var representative = times.Where(t => t <= best * 1.07).ToList();
            lapsUsed = representative.Count;
            if (representative.Count < 2) return 0;

            double mean = representative.Average();
            double variance = representative.Sum(t => (t - mean) * (t - mean)) / (representative.Count - 1);
            return Math.Sqrt(variance) / 1000.0;
        }
    }
}
