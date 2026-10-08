using System;
using CommunityToolkit.Mvvm.ComponentModel;

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

    public class CornerComparison : ObservableObject
    {
        public int Number { get; set; }
        public string CornerName { get; set; } = string.Empty;
        public double ReferenceVmin { get; set; }
        public double SelectedVmin { get; set; }
        public double DeltaVmin => SelectedVmin - ReferenceVmin;
        public string DeltaColor => DeltaVmin < -1 ? "#ef4444" : (DeltaVmin > 1 ? "#22c55e" : "#ffffff");

        // Points GPS où la Vmin est atteinte (null si aucun point dans le virage)
        public TelemetryPoint? ReferenceVminPoint { get; set; }
        public TelemetryPoint? SelectedVminPoint { get; set; }

        // Positions projetées sur la carte (coordonnées du canvas 500x500)
        private double _mapX;
        public double MapX { get => _mapX; set => SetProperty(ref _mapX, value); }

        private double _mapY;
        public double MapY { get => _mapY; set => SetProperty(ref _mapY, value); }

        private double _refMapX;
        public double RefMapX { get => _refMapX; set => SetProperty(ref _refMapX, value); }

        private double _refMapY;
        public double RefMapY { get => _refMapY; set => SetProperty(ref _refMapY, value); }

        private bool _hasMapPosition;
        public bool HasMapPosition { get => _hasMapPosition; set => SetProperty(ref _hasMapPosition, value); }

        private bool _hasRefMapPosition;
        public bool HasRefMapPosition { get => _hasRefMapPosition; set => SetProperty(ref _hasRefMapPosition, value); }

        // Virage sélectionné dans le tableau Vmin : mis en évidence sur la carte
        private bool _isHighlighted;
        public bool IsHighlighted { get => _isHighlighted; set => SetProperty(ref _isHighlighted, value); }
    }
}
