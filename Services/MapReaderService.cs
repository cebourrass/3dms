using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Analyzer.Models;

namespace Analyzer.Services
{
    public class TrackMap
    {
        public string Name { get; set; }
        public List<GpsPoint> Trajectory { get; set; } = new List<GpsPoint>();
        public Dictionary<string, GpsPoint> Markers { get; set; } = new Dictionary<string, GpsPoint>();
    }

    public class GpsPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class MapReaderService
    {
        public TrackMap ReadMap(string filePath)
        {
            var map = new TrackMap { Name = Path.GetFileNameWithoutExtension(filePath) };

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // Header check ".MAP.1.0"
                byte[] header = reader.ReadBytes(8);
                // On saute les 4 doubles du début (vraisemblablement des infos de bordure/centre)
                reader.ReadBytes(32);

                int trajectoryCount = reader.ReadInt32();
                int markerCount = reader.ReadInt32();

                // Lecture des marqueurs
                for (int i = 0; i < markerCount; i++)
                {
                    int nameLen = reader.ReadByte();
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(nameLen));
                    double lon = reader.ReadDouble();
                    double lat = reader.ReadDouble();
                    map.Markers[name] = new GpsPoint { Latitude = lat, Longitude = lon };
                }

                // Lecture de la trajectoire
                for (int i = 0; i < trajectoryCount; i++)
                {
                    double lon = reader.ReadDouble();
                    double lat = reader.ReadDouble();
                    map.Trajectory.Add(new GpsPoint { Latitude = lat, Longitude = lon });
                }
            }

            return map;
        }
    }
}
