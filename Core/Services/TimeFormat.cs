using System.Globalization;
using System.Text.RegularExpressions;

namespace Analyzer.Services
{
    /// <summary>
    /// Conversions des temps au tour ("mm:ss.cc") et des dates de session.
    /// </summary>
    public static class TimeFormat
    {
        /// <summary>"01:06.65" → 66650 ms. Retourne 0 si la valeur est absente ou invalide.</summary>
        public static double ParseToMs(string? timeStr)
        {
            if (string.IsNullOrEmpty(timeStr) || timeStr == "-") return 0;
            try
            {
                var parts = timeStr.Split(':');
                if (parts.Length < 2) return 0;

                double minutes = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double seconds = double.Parse(parts[1], CultureInfo.InvariantCulture);
                return (minutes * 60 + seconds) * 1000.0;
            }
            catch { return 0; }
        }

        /// <summary>66650 ms → "01:06.65".</summary>
        public static string FormatMs(double ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            return string.Format("{0:D2}:{1:D2}.{2:D2}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }

        /// <summary>
        /// Date d'une session à partir de son nom de fichier ("2026-07-24 a 15h03", "CIRCUIT-2026-07-24", ...).
        /// Retourne null si aucune date n'est reconnue.
        /// </summary>
        public static DateTime? ParseSessionDate(string fileName)
        {
            try
            {
                var match = Regex.Match(fileName, @"(\d{4})-(\d{2})-(\d{2})\D{1,4}(\d{2})[hH](\d{2})");
                if (match.Success)
                {
                    return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                                        int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value), 0);
                }

                var dateMatch = Regex.Match(fileName, @"(\d{4})-(\d{2})-(\d{2})");
                if (dateMatch.Success)
                {
                    return new DateTime(int.Parse(dateMatch.Groups[1].Value), int.Parse(dateMatch.Groups[2].Value), int.Parse(dateMatch.Groups[3].Value));
                }
            }
            catch { /* date invalide */ }

            return null;
        }
    }
}
