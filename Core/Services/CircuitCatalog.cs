using System.Globalization;
using System.Text;
using Analyzer.Models;

namespace Analyzer.Services
{
    /// <summary>
    /// Bibliothèque des circuits (.map) et détection du circuit d'une session.
    /// </summary>
    public class CircuitCatalog
    {
        private readonly MapReaderService _mapReader = new();

        public List<CircuitMetadata> Circuits { get; } = new();

        /// <summary>Charge tous les .map de <paramref name="mapsRoot"/> (un sous-dossier par pays), triés par nom.</summary>
        public void Load(string mapsRoot)
        {
            Circuits.Clear();
            if (!Directory.Exists(mapsRoot)) return;

            foreach (var countryDir in Directory.GetDirectories(mapsRoot))
            {
                foreach (var mapFile in Directory.GetFiles(countryDir, "*.map"))
                {
                    try
                    {
                        var map = _mapReader.ReadMap(mapFile);
                        var startMarker = map.Markers.FirstOrDefault(m => m.Key.StartsWith("Start", StringComparison.OrdinalIgnoreCase)).Value;
                        Circuits.Add(new CircuitMetadata { Name = map.Name, FilePath = mapFile, StartPoint = startMarker });
                    }
                    catch { /* Fichier .map corrompu : ignoré */ }
                }
            }

            Circuits.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));
        }

        /// <summary>
        /// Circuit dont la ligne de départ est la plus proche du premier point GPS (à moins de 5 km).
        /// </summary>
        public CircuitMetadata? Detect(IReadOnlyList<TelemetryPoint> points)
        {
            if (points.Count == 0 || Circuits.Count == 0) return null;

            var firstPoint = points[0];
            CircuitMetadata? bestMatch = null;
            double minDistance = double.MaxValue;

            foreach (var circuit in Circuits)
            {
                if (circuit.StartPoint == null) continue;

                double dist = Geo.DistanceMeters(firstPoint.Latitude, firstPoint.Longitude,
                                                 circuit.StartPoint.Latitude, circuit.StartPoint.Longitude);
                if (dist < 5000 && dist < minDistance)
                {
                    minDistance = dist;
                    bestMatch = circuit;
                }
            }

            if (bestMatch != null && bestMatch.Name.Contains("Alès"))
            {
                // Priorité par défaut au sens horaire pour Alès
                var horaire = Circuits.FirstOrDefault(c => c.Name == "Alès (Sens horaire)");
                if (horaire != null) return horaire;
            }

            return bestMatch;
        }

        /// <summary>
        /// Repli par nom : le dossier de la session (ex. "LEDENON-2026-04-12") est comparé aux noms de circuits.
        /// </summary>
        public CircuitMetadata? FindByFolderName(string sessionFilePath)
        {
            string sessionDir = Path.GetDirectoryName(sessionFilePath) ?? "";
            string circuitSearch = Normalize(Path.GetFileName(sessionDir).Split('-')[0]);
            if (circuitSearch.Length == 0) return null;

            return Circuits.FirstOrDefault(c =>
            {
                string mapName = Normalize(Path.GetFileNameWithoutExtension(c.FilePath));
                return mapName.Contains(circuitSearch) || circuitSearch.Contains(mapName);
            });
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Sans accents, en minuscules
            return new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Normalize(NormalizationForm.FormC)
                .ToLower();
        }
    }

    internal static class Geo
    {
        /// <summary>Distance haversine en mètres.</summary>
        public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371.0 * c * 1000.0;
        }
    }
}
