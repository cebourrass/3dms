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

                    points.Add(point);
                }
            }

            return points;
        }
    }
}
