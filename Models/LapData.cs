using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Analyzer.Models
{
    public class LapData : ObservableObject
    {
        public int Number { get; set; }
        public string Type { get; set; }
        public string CumulativeTime { get; set; }
        public string LapTime { get; set; }
        
        // Données précises pour l'interpolation
        public double StartTimeMs { get; set; }
        public double StartDistance { get; set; }
        public double LapTimeMs { get; set; }

        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }
        public double MaxLeanLeft { get; set; }
        public double MaxLeanRight { get; set; }
        public float MaxAccel { get; set; }
        public float MaxDecel { get; set; }
        public string[] Partials { get; set; }
        public double[] PartialDistances { get; set; }
        public double[] CumulativePartialTimesMs { get; set; }
        
        // Stockage optionnel des points (nécessaire pour la référence globale entre sessions)
        public System.Collections.Generic.List<TelemetryPoint>? TelemetryPoints { get; set; }

        private bool _isBestLap;
        public bool IsBestLap { get => _isBestLap; set => SetProperty(ref _isBestLap, value); }
        
        private bool _isMaxSpeed;
        public bool IsMaxSpeed { get => _isMaxSpeed; set => SetProperty(ref _isMaxSpeed, value); }
        
        private bool _isMaxAngle;
        public bool IsMaxAngle { get => _isMaxAngle; set => SetProperty(ref _isMaxAngle, value); }

        private bool _isMinSpeed;
        public bool IsMinSpeed { get => _isMinSpeed; set => SetProperty(ref _isMinSpeed, value); }

        private bool _isMaxAccel;
        public bool IsMaxAccel { get => _isMaxAccel; set => SetProperty(ref _isMaxAccel, value); }

        private bool _isReference;
        public bool IsReference { get => _isReference; set => SetProperty(ref _isReference, value); }

        private bool _isCompared;
        public bool IsCompared { get => _isCompared; set => SetProperty(ref _isCompared, value); }
    }
}
