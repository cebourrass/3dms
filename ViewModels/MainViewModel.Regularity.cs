using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Analyzer.Services;
using Analyzer.Models;
using System.Collections.Generic;
using System.IO;

namespace Analyzer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<RegularityItem> RegularityStats { get; } = new();

        private bool _isRegularityVisible;
        public bool IsRegularityVisible { get => _isRegularityVisible; set => SetProperty(ref _isRegularityVisible, value); }

        private double _regularityThresholdExcellent = 0.10;
        public double RegularityThresholdExcellent 
        { 
            get => _regularityThresholdExcellent; 
            set 
            { 
                if (SetProperty(ref _regularityThresholdExcellent, value)) 
                { 
                    if (!_isApplyingProfile) SelectedPilotProfile = PilotProfiles.Last();
                    UpdateRegularityStats(); 
                } 
            } 
        }

        private double _regularityThresholdMedium = 0.30;
        public double RegularityThresholdMedium 
        { 
            get => _regularityThresholdMedium; 
            set 
            { 
                if (SetProperty(ref _regularityThresholdMedium, value)) 
                { 
                    if (!_isApplyingProfile) SelectedPilotProfile = PilotProfiles.Last();
                    UpdateRegularityStats(); 
                } 
            } 
        }

        public List<PilotProfile> PilotProfiles { get; } = new()
        {
            new PilotProfile { Name = "Expert / Pro", Excellent = 0.05, Medium = 0.15 },
            new PilotProfile { Name = "Confirmé / Régulier", Excellent = 0.10, Medium = 0.30 },
            new PilotProfile { Name = "Intermédiaire", Excellent = 0.25, Medium = 0.60 },
            new PilotProfile { Name = "Débutant", Excellent = 0.50, Medium = 1.20 },
            new PilotProfile { Name = "Personnalisé", Excellent = 0, Medium = 0 }
        };

        private bool _isApplyingProfile = false;
        private PilotProfile? _selectedPilotProfile;
        public PilotProfile? SelectedPilotProfile
        {
            get => _selectedPilotProfile;
            set
            {
                if (SetProperty(ref _selectedPilotProfile, value) && value != null)
                {
                    if (value.Name != "Personnalisé")
                    {
                        _isApplyingProfile = true;
                        RegularityThresholdExcellent = value.Excellent;
                        RegularityThresholdMedium = value.Medium;
                        _isApplyingProfile = false;
                    }
                }
            }
        }

        private void UpdateRegularityStats()
        {
            RegularityStats.Clear();
            
            var lapPool = new HashSet<LapData>();
            if (SelectedLap != null && SelectedLap.LapTimeMs > 0) lapPool.Add(SelectedLap);
            if (ShowReference && ReferenceLap != null && ReferenceLap.LapTimeMs > 0) lapPool.Add(ReferenceLap);
            foreach (var lap in ComparisonLaps) if (lap.LapTimeMs > 0) lapPool.Add(lap);

            var laps = lapPool.ToList();
            
            if (laps.Count < 2)
            {
                IsRegularityVisible = false;
                return;
            }

            IsRegularityVisible = true;
            AddRegularityItem("Total", laps.Select(l => (double)l.LapTimeMs / 1000.0).ToList());

            int numSectors = 0;
            if (laps.Any()) numSectors = laps.Max(l => l.Partials?.Length ?? 0);

            for (int i = 0; i < numSectors; i++)
            {
                int sectorIndex = i;
                var sectorTimes = laps
                    .Where(l => l.Partials != null && sectorIndex < l.Partials.Length)
                    .Select(l => TimeFormat.ParseToMs(l.Partials[sectorIndex]) / 1000.0)
                    .Where(t => t > 0)
                    .ToList();

                if (sectorTimes.Count >= 2)
                {
                    AddRegularityItem($"P{i + 1}", sectorTimes);
                }
            }
        }

        private void AddRegularityItem(string label, List<double> values)
        {
            double avg = values.Average();
            double sumSquares = values.Select(v => Math.Pow(v - avg, 2)).Sum();
            double stdDev = Math.Sqrt(sumSquares / values.Count);

            string color = "#10b981"; // Vert
            if (stdDev > RegularityThresholdMedium) color = "#ef4444"; // Rouge
            else if (stdDev > RegularityThresholdExcellent) color = "#f59e0b"; // Orange

            RegularityStats.Add(new RegularityItem { Label = label, StdDev = stdDev, Color = color });
        }
    }
}
