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
            double lastCrossingTimeMs = points[0].Time;
            int lastCrossingIndex = 0;
            
            for (int i = 1; i < points.Count; i++)
            {
                var p1 = points[i - 1];
                var p2 = points[i];
                
                double d1 = CalculateDistance(p1.Latitude, p1.Longitude, startPoint.Latitude, startPoint.Longitude);
                double d2 = CalculateDistance(p2.Latitude, p2.Longitude, startPoint.Latitude, startPoint.Longitude);

                // Détection de passage : on est passé au plus près du marqueur
                // On utilise une détection de "passage au zenith" (passage par le point de distance minimale)
                // ou simplement si on est dans la zone et que la distance commence à remonter.
                if (d1 < CrossingThresholdMeters && d2 > d1 && i > lastCrossingIndex + 50) 
                {
                    // Interpolation temporelle exacte
                    double exactTime = GetInterpolatedTime(p1, p2, startPoint);
                    
                    AddLap(laps, points, lastCrossingIndex, i-1, currentLapNumber++, map, exactTime, lastCrossingTimeMs);
                    
                    lastCrossingTimeMs = exactTime;
                    lastCrossingIndex = i-1;
                }
            }

            // Dernier tour incomplet
            if (lastCrossingIndex < points.Count - 1)
            {
                AddLap(laps, points, lastCrossingIndex, points.Count - 1, currentLapNumber, map, points.Last().Time, lastCrossingTimeMs, isPartial: true);
            }

            return laps;
        }

        private void AddLap(List<LapData> laps, List<TelemetryPoint> allPoints, int startIndex, int endIndex, int number, TrackMap map, double endTimeMs, double startTimeMs, bool isPartial = false)
        {
            var lapPoints = allPoints.GetRange(startIndex, endIndex - startIndex + 1);
            if (lapPoints.Count == 0) return;

            double durationMs = endTimeMs - startTimeMs;
            double cumulativeMs = endTimeMs - allPoints[0].Time;

            var lap = new LapData
            {
                Number = number,
                Type = isPartial ? "Incomplet" : "Complet",
                CumulativeTime = FormatTime((long)cumulativeMs),
                LapTime = isPartial ? "-" : FormatTime((long)durationMs),
                LapTimeMs = durationMs,
                StartTimeMs = startTimeMs,
                MaxSpeed = lapPoints.Max(p => p.Speed),
                MinSpeed = lapPoints.Min(p => p.Speed),
                MaxLeanLeft = lapPoints.Max(p => p.LeanAngle < 0 ? -p.LeanAngle : 0),
                MaxLeanRight = lapPoints.Max(p => p.LeanAngle > 0 ? p.LeanAngle : 0),
                MaxAccel = lapPoints.Max(p => p.Acceleration),
                MaxDecel = Math.Abs(lapPoints.Min(p => p.Acceleration)),
            };

            // Détection des Partiels (P1, P2...) avec Interpolation
            var partialMarkers = map.Markers
                .Where(m => m.Key.StartsWith("Time", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Key)
                .ToList();

            int partialCount = Math.Max(4, partialMarkers.Count + 1);
            lap.Partials = new string[partialCount];
            for (int i = 0; i < partialCount; i++) lap.Partials[i] = "-";

            double previousTimeMs = startTimeMs;
            bool allPartialsFound = true;

            for (int pIdx = 0; pIdx < partialMarkers.Count; pIdx++)
            {
                var marker = partialMarkers[pIdx].Value;
                double minDist = double.MaxValue;
                int bestIdx = -1;

                for (int i = startIndex; i <= endIndex - 1; i++)
                {
                    double d = CalculateDistance(allPoints[i].Latitude, allPoints[i].Longitude, marker.Latitude, marker.Longitude);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestIdx = i;
                    }
                }

                if (bestIdx != -1 && minDist < CrossingThresholdMeters)
                {
                    double exactSplitTimeMs = GetInterpolatedTime(allPoints[bestIdx], allPoints[bestIdx+1], marker);
                    double splitDuration = exactSplitTimeMs - previousTimeMs;
                    lap.Partials[pIdx] = FormatTime((long)splitDuration);
                    previousTimeMs = exactSplitTimeMs;
                }
                else
                {
                    allPartialsFound = false;
                }
            }

            // DERNIER SECTEUR
            if (!isPartial)
            {
                double lastSplitMs = endTimeMs - previousTimeMs;
                if (partialMarkers.Count < partialCount)
                {
                    lap.Partials[partialMarkers.Count] = FormatTime((long)lastSplitMs);
                }
            }

            // Validation finale : un tour n'est complet que si on a tout franchi et qu'il n'est pas marqué partial
            if (isPartial || !allPartialsFound)
            {
                lap.Type = "Incomplet";
                if (isPartial) lap.LapTime = "-";
            }
            else
            {
                lap.Type = "Complet";
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

        private double GetInterpolatedTime(TelemetryPoint p1, TelemetryPoint p2, GpsPoint marker)
        {
            // Calcul de la distance au marqueur pour les deux points
            double d1 = CalculateDistance(p1.Latitude, p1.Longitude, marker.Latitude, marker.Longitude);
            double d2 = CalculateDistance(p2.Latitude, p2.Longitude, marker.Latitude, marker.Longitude);

            // Si les distances sont identiques (rare), on prend le milieu
            if (Math.Abs(d1 + d2) < 0.000001) return p1.Time;

            // Interpolation de base (fraction de temps basée sur la distance)
            double fraction = d1 / (d1 + d2);

            // Prise en compte de l'accélération pour affiner la position temporelle
            // Si a > 0, on met un peu plus de temps à parcourir la première partie du segment
            double accelG = (p1.Acceleration + p2.Acceleration) / 2.0;
            double accelMs2 = accelG * 9.81;
            
            // Correction quadratique simple du temps
            double deltaTimeMs = p2.Time - p1.Time;
            double correction = 0;
            
            if (Math.Abs(accelMs2) > 0.1)
            {
                // On ajuste légèrement la fraction selon si on accélère ou freine
                // C'est une approximation cinématique : t = t1 + dt * fraction + correction_accel
                correction = -0.5 * accelMs2 * (deltaTimeMs / 1000.0) * fraction * (1.0 - fraction);
            }

            return p1.Time + (deltaTimeMs * fraction) + (correction * 1000.0);
        }

        private double ToRadians(double deg) => deg * Math.PI / 180.0;

        private string FormatTime(long ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            return string.Format("{0:D2}:{1:D2}.{2:D2}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }
    }
}
