using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer.Models;

namespace Analyzer.Services
{
    public class CornerService
    {
        public List<Corner> DetectCorners(IEnumerable<TelemetryPoint> points)
        {
            var corners = new List<Corner>();
            var pointList = points.ToList();
            if (pointList.Count < 10) return corners;

            double lapStartDist = pointList[0].Distance;

            bool inCorner = false;
            double cornerStartDist = 0;
            double maxAngleInCorner = 0;
            double apexDist = 0;
            
            for (int i = 0; i < pointList.Count; i++)
            {
                var p = pointList[i];
                double absAngle = Math.Abs(p.LeanAngle);
                double relativeDist = p.Distance - lapStartDist;

                if (!inCorner && absAngle > 15)
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

                    if (absAngle < 8) // Sortie de virage
                    {
                        if (relativeDist - cornerStartDist > 30) // Minimum 30m pour un virage
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

        public List<CornerComparison> CompareLaps(LapData reference, LapData selected, List<Corner> corners)
        {
            var comparisons = new List<CornerComparison>();
            if (reference == null || selected == null || reference.TelemetryPoints == null || selected.TelemetryPoints == null) 
                return comparisons;

            foreach (var corner in corners)
            {
                double refVmin = GetVminInRelativeRange(reference.TelemetryPoints, reference.StartDistance, corner.StartDistance, corner.EndDistance);
                double selVmin = GetVminInRelativeRange(selected.TelemetryPoints, selected.StartDistance, corner.StartDistance, corner.EndDistance);

                comparisons.Add(new CornerComparison
                {
                    CornerName = corner.Name,
                    ReferenceVmin = refVmin,
                    SelectedVmin = selVmin
                });
            }

            return comparisons;
        }

        private double GetVminInRelativeRange(IEnumerable<TelemetryPoint> points, double lapStartAbsDist, double startRel, double endRel)
        {
            // On cherche les points dont la distance relative (p.Distance - lapStartAbsDist) est dans l'intervalle
            var range = points.Where(p => {
                double rel = p.Distance - lapStartAbsDist;
                return rel >= startRel && rel <= endRel;
            }).ToList();

            if (!range.Any()) return 0;
            return range.Min(p => p.Speed);
        }
    }
}
