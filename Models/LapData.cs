using System;

namespace Analyzer.Models
{
    public class LapData
    {
        public int Number { get; set; }
        public string Type { get; set; } = "Complet";
        public string CumulativeTime { get; set; }
        public string LapTime { get; set; }
        public float MaxSpeed { get; set; }
        public float MinSpeed { get; set; }
        public float MaxLeanLeft { get; set; }
        public float MaxLeanRight { get; set; }
        public float MaxAccel { get; set; }
        public float MaxDecel { get; set; }
        public string[] Partials { get; set; }
    }
}
