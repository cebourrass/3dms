using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer.Models;

namespace Analyzer.Services
{
    public class LapService
    {
        private const double EarthRadiusKm = 6371.0;
        private const double CrossingThresholdMeters = 25.0;

        public List<LapData> CalculateLaps(List<TelemetryPoint> points, TrackMap map)
        {
            var laps = new List<LapData>();
            string? startKey = map?.Markers.Keys.FirstOrDefault(k => k.StartsWith("Start", StringComparison.OrdinalIgnoreCase));
            
            if (points == null || points.Count < 2 || map == null || startKey == null)
                return laps;

            var startPoint = map.Markers[startKey];
            int currentLapNumber = 1;
            int lastCrossingIndex = 0;
            
            bool inZone = false;
            double minDistInZone = double.MaxValue;
            int bestIndexInZone = -1;

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                double dist = CalculateDistance(p.Latitude, p.Longitude, startPoint.Latitude, startPoint.Longitude);

                if (dist < CrossingThresholdMeters)
                {
                    inZone = true;
                    if (dist < minDistInZone)
                    {
                        minDistInZone = dist;
                        bestIndexInZone = i;
                    }
                }
                else if (inZone)
                {
                    // On vient de sortir de la zone de franchissement
                    if (bestIndexInZone != -1)
                    {
                        AddLap(laps, points, lastCrossingIndex, bestIndexInZone, currentLapNumber++, map);
                        lastCrossingIndex = bestIndexInZone;
                    }
                    inZone = false;
                    minDistInZone = double.MaxValue;
                    bestIndexInZone = -1;
                }
            }

            // Dernier tour incomplet
            if (lastCrossingIndex < points.Count - 1)
            {
                AddLap(laps, points, lastCrossingIndex, points.Count - 1, currentLapNumber, map, isPartial: true);
            }

            return laps;
        }

        private void AddLap(List<LapData> laps, List<TelemetryPoint> allPoints, int startIndex, int endIndex, int number, TrackMap map, bool isPartial = false)
        {
            var lapPoints = allPoints.GetRange(startIndex, endIndex - startIndex + 1);
            if (lapPoints.Count == 0) return;

            long durationMs = allPoints[endIndex].Time - allPoints[startIndex].Time;
            long cumulativeMs = allPoints[endIndex].Time - allPoints[0].Time;

            var lap = new LapData
            {
                Number = number,
                Type = isPartial ? "Incomplet" : "Complet",
                CumulativeTime = FormatTime(cumulativeMs),
                LapTime = isPartial ? "-" : FormatTime(durationMs),
                MaxSpeed = lapPoints.Max(p => p.Speed),
                MinSpeed = lapPoints.Min(p => p.Speed),
                MaxLeanLeft = lapPoints.Max(p => p.LeanAngle < 0 ? -p.LeanAngle : 0),
                MaxLeanRight = lapPoints.Max(p => p.LeanAngle > 0 ? p.LeanAngle : 0),
                MaxAccel = lapPoints.Max(p => p.Acceleration),
                MaxDecel = Math.Abs(lapPoints.Min(p => p.Acceleration)),
            };

            // Détection des Partiels (P1, P2...)
            var partialMarkers = map.Markers
                .Where(m => m.Key.StartsWith("Time", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Key)
                .ToList();

            // On Dimensionne à au moins 4 pour le DataGrid (P1 à P4), ou plus si nécessaire
            int partialCount = Math.Max(4, partialMarkers.Count + 1);
            lap.Partials = new string[partialCount];
            for (int i = 0; i < partialCount; i++) lap.Partials[i] = "-";

            long previousTime = allPoints[startIndex].Time;

            for (int pIdx = 0; pIdx < partialMarkers.Count; pIdx++)
            {
                var marker = partialMarkers[pIdx].Value;
                double minDist = double.MaxValue;
                int bestIdx = -1;

                for (int i = 0; i < lapPoints.Count; i++)
                {
                    double d = CalculateDistance(lapPoints[i].Latitude, lapPoints[i].Longitude, marker.Latitude, marker.Longitude);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestIdx = i;
                    }
                }

                if (bestIdx != -1 && minDist < CrossingThresholdMeters)
                {
                    long currentTime = lapPoints[bestIdx].Time;
                    long splitMs = currentTime - previousTime;
                    lap.Partials[pIdx] = FormatTime(splitMs);
                    previousTime = currentTime;
                }
            }

            // DERNIER SECTEUR : Du dernier marqueur Time jusqu'à la fin (Ligne d'arrivée)
            if (lap.Type == "Complet")
            {
                long finishTime = allPoints[endIndex].Time;
                long lastSplitMs = finishTime - previousTime;
                if (partialMarkers.Count < partialCount)
                {
                    lap.Partials[partialMarkers.Count] = FormatTime(lastSplitMs);
                }
            }

            laps.Add(lap);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c * 1000.0; // En mètres
        }

        private double ToRadians(double deg) => deg * Math.PI / 180.0;

        private string FormatTime(long ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            return string.Format("{0:D2}:{1:D2}.{2:D2}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }
    }
}
