using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer.Models;

namespace Analyzer.Services
{
    /// <summary>
    /// Seuils de détection des virages (réglables dans PARAMÈTRES).
    /// </summary>
    public record CornerDetectionSettings(double EntryAngle = 15, double ExitAngle = 8, double MinLength = 30);

    public class CornerService
    {
        public List<Corner> DetectCorners(IEnumerable<TelemetryPoint> points, CornerDetectionSettings settings)
        {
            var corners = new List<Corner>();
            var pointList = points.ToList();
            if (pointList.Count < 10) return corners;

            double lapStartDist = pointList[0].Distance;
            // La sortie doit rester sous l'entrée, sinon on entrerait et sortirait au même point
            double exitAngle = Math.Min(settings.ExitAngle, settings.EntryAngle);

            bool inCorner = false;
            double cornerStartDist = 0;
            double maxAngleInCorner = 0;
            double apexDist = 0;

            for (int i = 0; i < pointList.Count; i++)
            {
                var p = pointList[i];
                double absAngle = Math.Abs(p.LeanAngle);
                double relativeDist = p.Distance - lapStartDist;

                if (!inCorner && absAngle > settings.EntryAngle)
                {
                    inCorner = true;
                    cornerStartDist = relativeDist;
                    maxAngleInCorner = absAngle;
                    apexDist = relativeDist;
                }
                else if (inCorner)
                {
                    if (absAngle > maxAngleInCorner)
                    {
                        maxAngleInCorner = absAngle;
                        apexDist = relativeDist;
                    }

                    if (absAngle < exitAngle) // Sortie de virage
                    {
                        if (relativeDist - cornerStartDist > settings.MinLength)
                        {
                            corners.Add(new Corner
                            {
                                Id = corners.Count + 1,
                                Name = $"Virage {corners.Count + 1}",
                                StartDistance = cornerStartDist,
                                EndDistance = relativeDist,
                                ApexDistance = apexDist
                            });
                        }
                        inCorner = false;
                        maxAngleInCorner = 0;
                    }
                }
            }

            return corners;
        }

        /// <summary>
        /// Vmin de chaque virage sur le tour sélectionné, comparée à la référence si elle est fournie.
        /// </summary>
        public List<CornerComparison> CompareLaps(LapData? reference, LapData selected, List<Corner> corners)
        {
            var comparisons = new List<CornerComparison>();
            if (selected?.TelemetryPoints == null)
                return comparisons;

            foreach (var corner in corners)
            {
                var refPoint = reference?.TelemetryPoints == null ? null
                    : GetVminPointInRelativeRange(reference.TelemetryPoints, reference.StartDistance, corner.StartDistance, corner.EndDistance);
                var selPoint = GetVminPointInRelativeRange(selected.TelemetryPoints, selected.StartDistance, corner.StartDistance, corner.EndDistance);

                comparisons.Add(new CornerComparison
                {
                    Number = corner.Id,
                    CornerName = corner.Name,
                    ReferenceVmin = refPoint?.Speed,
                    SelectedVmin = selPoint?.Speed ?? 0,
                    ReferenceVminPoint = refPoint,
                    SelectedVminPoint = selPoint
                });
            }

            return comparisons;
        }

        private TelemetryPoint? GetVminPointInRelativeRange(IEnumerable<TelemetryPoint> points, double lapStartAbsDist, double startRel, double endRel)
        {
            // On cherche les points dont la distance relative (p.Distance - lapStartAbsDist) est dans l'intervalle
            return points
                .Where(p => {
                    double rel = p.Distance - lapStartAbsDist;
                    return rel >= startRel && rel <= endRel;
                })
                .MinBy(p => p.Speed);
        }
    }
}
