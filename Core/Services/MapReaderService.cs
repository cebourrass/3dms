using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Analyzer.Models;

namespace Analyzer.Services
{
    public class MapReaderService
    {
        public TrackMap ReadMap(string filePath)
        {
            var map = new TrackMap { Name = Path.GetFileNameWithoutExtension(filePath) };

            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // Sauter le header (8 octets) et les infos (32 octets)
                if (reader.BaseStream.Length > 40) reader.ReadBytes(40);
                
                long fileSize = reader.BaseStream.Length;
                map.Orientation = reader.ReadInt32();     // Offset 40 : Orientation (0, 90, 180, 270)
                int markerCount = reader.ReadInt32();     // Offset 44 : Nombre de Marqueurs

                // Sécurité contre les fichiers corrompus
                if (markerCount < 0 || markerCount > 100) markerCount = 0; 

                // Lecture des marqueurs
                for (int i = 0; i < markerCount; i++)
                {
                    if (reader.BaseStream.Position >= fileSize) break;
                    int nameLen = reader.ReadByte();
                    if (nameLen > 0 && reader.BaseStream.Position + nameLen < fileSize)
                    {
                        string name = Encoding.ASCII.GetString(reader.ReadBytes(nameLen));
                        double lon = reader.ReadDouble();
                        double lat = reader.ReadDouble();
                        map.Markers[name] = new GpsPoint { Latitude = lat, Longitude = lon };
                    }
                }
                
                // Lecture de la largeur et du nombre de points de la trajectoire
                int trajectoryCount = 0;
                if (reader.BaseStream.Position + 12 <= fileSize)
                {
                    map.TrackWidth = reader.ReadDouble();
                    trajectoryCount = reader.ReadInt32(); // Nombre de points
                }

                // Sécurité trajectoryCount
                if (trajectoryCount < 0 || trajectoryCount > (fileSize / 16)) trajectoryCount = 0;

                // Lecture de la trajectoire
                for (int i = 0; i < trajectoryCount; i++)
                {
                    if (reader.BaseStream.Position + 16 > fileSize) break;
                    double lon = reader.ReadDouble();
                    double lat = reader.ReadDouble();
                    map.Trajectory.Add(new GpsPoint { Latitude = lat, Longitude = lon });
                }
            }

            return map;
        }
    }
}
