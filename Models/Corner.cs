using System;

namespace Analyzer.Models
{
    public class Corner
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double StartDistance { get; set; }
        public double EndDistance { get; set; }
        public double ApexDistance { get; set; }
    }

    public class CornerComparison
    {
        public string CornerName { get; set; } = string.Empty;
        public double ReferenceVmin { get; set; }
        public double SelectedVmin { get; set; }
        public double DeltaVmin => SelectedVmin - ReferenceVmin;
        public string DeltaColor => DeltaVmin < -1 ? "#ef4444" : (DeltaVmin > 1 ? "#22c55e" : "#ffffff");
    }
}
