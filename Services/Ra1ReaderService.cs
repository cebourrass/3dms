using System;
using System.Collections.Generic;
using System.IO;
using Analyzer.Models;

namespace Analyzer.Services
{
    /// <summary>
    /// Service permettant de lire et de parser les fichiers de données binaires au format .ra1 (3DMS).
    /// </summary>
    public class Ra1ReaderService
    {
        private const int HeaderSize = 16;
        private const int RecordSize = 28;

        /// <summary>
        /// Lit un fichier .ra1 et extrait la liste des points de télémétrie.
        /// </summary>
        /// <param name="filePath">Chemin d'accès complet au fichier .ra1.</param>
        /// <returns>Une liste d'objets <see cref="TelemetryPoint"/>.</returns>
        public int LastCorruptedPointsCount { get; private set; }

        public List<TelemetryPoint> ReadFile(string filePath)
        {
            var points = new List<TelemetryPoint>();
            LastCorruptedPointsCount = 0;

            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                if (stream.Length < HeaderSize)
                    return points;

                // Skip header (16 octets contenant la signature RA1 et la version)
                stream.Seek(HeaderSize, SeekOrigin.Begin);

                while (stream.Position + RecordSize <= stream.Length)
                {
                    var point = new TelemetryPoint
                    {
                        Time = reader.ReadUInt32(),
                        Longitude = reader.ReadSingle(),
                        Latitude = reader.ReadSingle(),
                        Speed = reader.ReadSingle(),
                        LeanAngle = reader.ReadSingle(),
                        Acceleration = reader.ReadSingle()
                    };

                    // Les fichiers .ra1 ont un champ réservé de 4 octets à la fin de chaque record
                    stream.Seek(4, SeekOrigin.Current);

                    // 1. Détecter si les données sont "physiquement impossibles" (vraie corruption qui fait crasher)
                    bool isCrazy = float.IsNaN(point.Latitude) || float.IsNaN(point.Longitude) || float.IsNaN(point.Speed) ||
                                 float.IsInfinity(point.Latitude) || float.IsInfinity(point.Longitude) || float.IsInfinity(point.Speed) ||
                                 Math.Abs(point.Latitude) > 90 || Math.Abs(point.Longitude) > 180 ||
                                 point.Speed > 600 || point.Speed < -10; // Marge pour le bruit capteur

                    // 2. Détecter si on a un fix GPS exploitable (les deux coordonnées doivent être présentes)
                    bool hasGpsFix = point.Latitude != 0 && point.Longitude != 0;

                    if (isCrazy)
                    {
                        // On ignore le signalement de corruption au début (init) ou à la toute fin (footer incomplet)
                        bool isNearEnd = (stream.Position + RecordSize > stream.Length);
                        if (points.Count > 10 && !isNearEnd)
                        {
                            LastCorruptedPointsCount++;
                        }
                        continue;
                    }

                    if (hasGpsFix)
                    {
                        if (points.Count > 0)
                        {
                            long timeDelta = (long)point.Time - (long)points.Last().Time;
                            
                            // Un saut de temps négatif ou de plus de 5 minutes est suspect (corruption RA1)
                            if (timeDelta < 0 || timeDelta > 300000) 
                            {
                                // Idem : on ignore silencieusement au tout début ou à la fin
                                bool isNearEnd = (stream.Position + RecordSize > stream.Length);
                                if (points.Count > 10 && !isNearEnd)
                                {
                                    LastCorruptedPointsCount++;
                                }
                                continue;
                            }
                        }
                        points.Add(point);
                    }
                    // Note : les points sans fix GPS (un ou deux coordonnées à zéro) sont ignorés silencieusement.
                }

                // Calcul des distances cumulées
                float totalDistance = 0;
                for (int i = 1; i < points.Count; i++)
                {
                    var p1 = points[i - 1];
                    var p2 = points[i];
                    totalDistance += (float)CalculateDistance(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude);
                    p2.Distance = totalDistance;
                }
            }

            return points;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371.0 * c * 1000.0; // En mètres
        }
    }
}
