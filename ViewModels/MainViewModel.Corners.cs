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
        private bool _showCornerMarkers = true;
        public bool ShowCornerMarkers { get => _showCornerMarkers; set => SetProperty(ref _showCornerMarkers, value); }

        // Seuils de détection des virages (analyse Vmin)
        private double _cornerEntryAngle = 15;
        public double CornerEntryAngle { get => _cornerEntryAngle; set { if (SetProperty(ref _cornerEntryAngle, value)) AnalyzeCorners(); } }

        private double _cornerExitAngle = 8;
        public double CornerExitAngle { get => _cornerExitAngle; set { if (SetProperty(ref _cornerExitAngle, value)) AnalyzeCorners(); } }

        private double _cornerMinLength = 30;
        public double CornerMinLength { get => _cornerMinLength; set { if (SetProperty(ref _cornerMinLength, value)) AnalyzeCorners(); } }

        // Virage sélectionné dans le tableau Vmin : mis en évidence sur la carte
        private CornerComparison? _selectedCornerComparison;
        public CornerComparison? SelectedCornerComparison
        {
            get => _selectedCornerComparison;
            set
            {
                var previous = _selectedCornerComparison;
                if (SetProperty(ref _selectedCornerComparison, value))
                {
                    if (previous != null) previous.IsHighlighted = false;
                    if (value != null) value.IsHighlighted = true;
                }
            }
        }

        private bool _isCornerAnalysisVisible;
        public bool IsCornerAnalysisVisible
        {
            get => _isCornerAnalysisVisible;
            set => SetProperty(ref _isCornerAnalysisVisible, value);
        }

        public ObservableCollection<CornerComparison> CornerComparisons { get; } = new();

        private void AnalyzeCorners()
        {
            if (ReferenceLap == null || SelectedLap == null || ReferenceLap.TelemetryPoints == null || SelectedLap.TelemetryPoints == null)
            {
                IsCornerAnalysisVisible = false;
                CornerComparisons.Clear();
                return;
            }

            var detection = new CornerDetectionSettings(CornerEntryAngle, CornerExitAngle, CornerMinLength);
            var corners = _cornerService.DetectCorners(ReferenceLap.TelemetryPoints, detection);
            var comparisons = _cornerService.CompareLaps(ReferenceLap, SelectedLap, corners);

            // Nom du virage dans la langue courante (ex. : "Virage 3" / "Corner 3")
            string prefix = System.Windows.Application.Current?.TryFindResource("ColCorner") as string ?? "Virage";
            foreach (var comp in comparisons) comp.CornerName = $"{prefix} {comp.Number}";

            // Conserver la sélection du même virage après un recalcul
            int? highlighted = SelectedCornerComparison?.Number;

            CornerComparisons.Clear();
            foreach (var comp in comparisons) CornerComparisons.Add(comp);
            ProjectCornerMarkers();

            SelectedCornerComparison = highlighted is int n ? CornerComparisons.FirstOrDefault(c => c.Number == n) : null;
            IsCornerAnalysisVisible = CornerComparisons.Any();
        }

        private void ProjectCornerMarkers()
        {
            foreach (var c in CornerComparisons)
            {
                c.HasMapPosition = _mapScale > 0 && c.SelectedVminPoint != null;
                if (c.HasMapPosition)
                {
                    var pt = ProjectToMap(c.SelectedVminPoint!);
                    c.MapX = pt.X;
                    c.MapY = pt.Y;
                }

                c.HasRefMapPosition = _mapScale > 0 && c.ReferenceVminPoint != null;
                if (c.HasRefMapPosition)
                {
                    var pt = ProjectToMap(c.ReferenceVminPoint!);
                    c.RefMapX = pt.X;
                    c.RefMapY = pt.Y;
                }
            }
        }
    }
}
