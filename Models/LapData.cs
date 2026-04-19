using System;

namespace Analyzer.Models
{
    public class LapData : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public int Number { get; set; }
        public string Type { get; set; }
        public string CumulativeTime { get; set; }
        public string LapTime { get; set; }
        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }
        public double MaxLeanLeft { get; set; }
        public double MaxLeanRight { get; set; }
        public float MaxAccel { get; set; }
        public float MaxDecel { get; set; }
        public string[] Partials { get; set; }

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
    }
}
