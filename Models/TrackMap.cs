using System.Collections.Generic;

namespace Analyzer.Models
{
    public class TrackMap
    {
        public string Name { get; set; } = string.Empty;
        public List<GpsPoint> Trajectory { get; set; } = new List<GpsPoint>();
        public Dictionary<string, GpsPoint> Markers { get; set; } = new Dictionary<string, GpsPoint>();
    }

    public class GpsPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
