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
        public List<TelemetryPoint> ReadFile(string filePath)
        {
            var points = new List<TelemetryPoint>();

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

                    if (point.Latitude != 0 && point.Longitude != 0)
                    {
                        points.Add(point);
                    }
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
