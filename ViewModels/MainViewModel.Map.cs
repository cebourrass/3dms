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
        private TrackMap? _currentMap;
        public TrackMap? CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

        private bool _showAccelMapGradient = false;
        public bool ShowAccelMapGradient { get => _showAccelMapGradient; set { if (SetProperty(ref _showAccelMapGradient, value)) UpdateTrajectoryUI(CurrentMap); } }

        private bool _autoCenterMap = true;
        public bool AutoCenterMap { get => _autoCenterMap; set => SetProperty(ref _autoCenterMap, value); }

        private double _accelGradientRange = 1.2;
        public double AccelGradientRange { get => _accelGradientRange; set { if (SetProperty(ref _accelGradientRange, value)) UpdateTrajectoryUI(CurrentMap); } }

        private bool _autoAccelGradientScaling = false;
        public bool AutoAccelGradientScaling { get => _autoAccelGradientScaling; set { if (SetProperty(ref _autoAccelGradientScaling, value)) UpdateTrajectoryUI(CurrentMap); } }

        // Comparison Map Thickness Range (Fast/Slow)
        private float _mapCompFastThickness = 1.0f;
        public float MapCompFastThickness { get => _mapCompFastThickness; set { if (SetProperty(ref _mapCompFastThickness, value)) UpdateTrajectoryUI(CurrentMap); } }
        private float _mapCompSlowThickness = 0.3f;
        public float MapCompSlowThickness { get => _mapCompSlowThickness; set { if (SetProperty(ref _mapCompSlowThickness, value)) UpdateTrajectoryUI(CurrentMap); } }

        private bool _showMapTooltip = true;
        public bool ShowMapTooltip { get => _showMapTooltip; set => SetProperty(ref _showMapTooltip, value); }

        private double _mapRotation = 0;
        public double MapRotation { get => _mapRotation; set => SetProperty(ref _mapRotation, value); }

        private double _mapZoom = 1.0;
        public double MapZoom { get => _mapZoom; set => SetProperty(ref _mapZoom, value); }

        [RelayCommand]
        public void ResetMapRotation()
        {
            MapRotation = 0;
        }

        [RelayCommand]
        public void ResetMapZoom()
        {
            MapZoom = 1.0;
            MapPanX = 0;
            MapPanY = 0;
        }

        private double _mapPanX = 0;
        public double MapPanX { get => _mapPanX; set => SetProperty(ref _mapPanX, value); }

        private double _mapPanY = 0;
        public double MapPanY { get => _mapPanY; set => SetProperty(ref _mapPanY, value); }

        [RelayCommand]
        public void ResetMapPan()
        {
            MapPanX = 0;
            MapPanY = 0;
        }

        private double _mapTrajectoryThickness = 1.2;
        public double MapTrajectoryThickness
        {
            get => _mapTrajectoryThickness;
            set { if (SetProperty(ref _mapTrajectoryThickness, value)) UpdateTrajectoryUI(CurrentSession?.CircuitMap); }
        }

        private double _mapCursorSize = 6.0;
        public double MapCursorSize
        {
            get => _mapCursorSize;
            set 
            { 
                if (SetProperty(ref _mapCursorSize, value))
                {
                    OnPropertyChanged(nameof(MapCursorOuterSize));
                    OnPropertyChanged(nameof(MapCursorMargin));
                    OnPropertyChanged(nameof(MapCursorOuterMargin));
                }
            }
        }

        public double MapCursorOuterSize => MapCursorSize * 1.8;
        public System.Windows.Thickness MapCursorMargin => new System.Windows.Thickness(-MapCursorSize / 2.0);
        public System.Windows.Thickness MapCursorOuterMargin => new System.Windows.Thickness(-MapCursorOuterSize / 2.0);
        public double MapCursorBlurRadius => MapCursorSize / 1.5;

        private double _cursorMapX;
        public double CursorMapX
        {
            get => _cursorMapX;
            set => SetProperty(ref _cursorMapX, value);
        }

        private double _cursorMapY;
        public double CursorMapY
        {
            get => _cursorMapY;
            set => SetProperty(ref _cursorMapY, value);
        }

        // Paramètres de projection pour la carte
        private double _mapScale;
        private double _mapRatio;
        private double _mapMinLat;
        private double _mapMinLon;
        private const double MapCanvasSize = 500;

        private System.Windows.Media.PointCollection _trajectoryPoints = new System.Windows.Media.PointCollection();
        public System.Windows.Media.PointCollection TrajectoryPoints
        {
            get => _trajectoryPoints;
            set => SetProperty(ref _trajectoryPoints, value);
        }

        public ObservableCollection<LapTrajectory> LapTrajectories { get; } = new();

        private double _trackWidthPixels = 5.0;
        public double TrackWidthPixels
        {
            get => _trackWidthPixels;
            set
            {
                if (SetProperty(ref _trackWidthPixels, value))
                {
                    OnPropertyChanged(nameof(TrackBorderWidthPixels));
                }
            }
        }

        public double TrackBorderWidthPixels => TrackWidthPixels + 2.0;
        private void UpdateTrajectoryUI(TrackMap? map)
        {
            if (map == null || map.Trajectory.Count == 0)
            {
                TrajectoryPoints = new();
                LapTrajectories.Clear();
                return;
            }

            // 1. Calculer la bounding box globale (basée sur le tracé de référence)
            double minLat = map.Trajectory.Min(p => p.Latitude);
            double maxLat = map.Trajectory.Max(p => p.Latitude);
            double minLon = map.Trajectory.Min(p => p.Longitude);
            double maxLon = map.Trajectory.Max(p => p.Longitude);

            double latRange = maxLat - minLat;
            double lonRange = maxLon - minLon;

            if (latRange == 0) latRange = 0.0001;
            if (lonRange == 0) lonRange = 0.0001;

            _mapMinLat = minLat;
            _mapMinLon = minLon;
            _mapRatio = Math.Cos(minLat * Math.PI / 180.0);
            double scaleLat = MapCanvasSize / latRange;
            double scaleLon = MapCanvasSize / (lonRange * _mapRatio);
            _mapScale = Math.Min(scaleLat, scaleLon) * 0.9;

            // Calcul de la largeur de piste en pixels (approx: 1° lat = 111111m)
            TrackWidthPixels = map.TrackWidth * (scaleLat / 111111.0);
            if (TrackWidthPixels < 2) TrackWidthPixels = 2;

            // 2. Projeter le tracé de référence
            var refPoints = new System.Windows.Media.PointCollection();
            foreach (var p in map.Trajectory)
            {
                double x = (p.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05);
                double y = MapCanvasSize - ((p.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05));
                refPoints.Add(new System.Windows.Point(x, y));
            }
            TrajectoryPoints = refPoints;

            // 3. Projeter les trajectoires des tours sélectionnés
            LapTrajectories.Clear();
            var lapsToDraw = new List<LapData>();
            if (SelectedLap != null) lapsToDraw.Add(SelectedLap);
            if (ShowReference && ReferenceLap != null && !lapsToDraw.Contains(ReferenceLap)) lapsToDraw.Add(ReferenceLap);
            foreach (var lap in ComparisonLaps) if (!lapsToDraw.Contains(lap)) lapsToDraw.Add(lap);

            // Calculer les bornes globales pour l'épaisseur dynamique (même logique que la télémétrie)
            double globalMinMs = 0;
            double globalMaxMs = 0;
            double globalRange = 0;
            bool isComparing = lapsToDraw.Count > 1;

            if (isComparing)
            {
                globalMinMs = lapsToDraw.Min(l => l.LapTimeMs);
                globalMaxMs = lapsToDraw.Max(l => l.LapTimeMs);
                globalRange = globalMaxMs - globalMinMs;
            }

            foreach (var lap in lapsToDraw)
            {
                var points = lap.TelemetryPoints;
                if (points == null || !points.Any()) 
                {
                    points = InterpolateAndSmooth(GetLapPoints(lap));
                    lap.TelemetryPoints = points;
                }

                // Calcul de l'épaisseur dynamique si comparaison
                double factor = (isComparing && globalRange > 0) ? (lap.LapTimeMs - globalMinMs) / globalRange : 0;
                double dynamicThickness = isComparing 
                    ? (MapCompFastThickness + (factor * (MapCompSlowThickness - MapCompFastThickness)))
                    : MapTrajectoryThickness;

                if (ShowAccelMapGradient)
                {
                    // --- MODE DÉGRADÉ D'ACCÉLÉRATION ---
                    
                    // Déterminer les limites pour ce tour
                    double limitPos = AccelGradientRange; // Freinage
                    double limitNeg = AccelGradientRange; // Accélération
                    
                    if (AutoAccelGradientScaling)
                    {
                        var allAccels = points.Select(p => p.Acceleration).ToList();
                        limitPos = (double)allAccels.Where(a => a > 0).DefaultIfEmpty(0.1f).Max();
                        limitNeg = (double)Math.Abs(allAccels.Where(a => a < 0).DefaultIfEmpty(-0.1f).Min());
                        
                        // Sécurité minimum pour éviter division par zero
                        if (limitPos < 0.1) limitPos = 0.1;
                        if (limitNeg < 0.1) limitNeg = 0.1;
                    }

                    // Optimisation : un segment tous les 'step' points pour fluidifier l'affichage
                    int step = 3; 
                    for (int i = 0; i < points.Count - step; i += step)
                    {
                        var p1 = points[i];
                        var p2 = points[i + step];
                        
                        var segPoints = new System.Windows.Media.PointCollection();
                        segPoints.Add(new System.Windows.Point(
                            (p1.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05),
                            MapCanvasSize - ((p1.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05))
                        ));
                        segPoints.Add(new System.Windows.Point(
                            (p2.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05),
                            MapCanvasSize - ((p2.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05))
                        ));

                        LapTrajectories.Add(new LapTrajectory
                        {
                            Color = GetColorForAccel(p2.Acceleration, limitPos, limitNeg),
                            Points = segPoints,
                            Thickness = lap == SelectedLap ? (dynamicThickness * 1.3) : dynamicThickness,
                            Opacity = lap == SelectedLap ? 1.0f : 0.6f
                        });
                    }
                }
                else
                {
                    // --- MODE STANDARD (Une Polyline unique par tour) ---
                    var projPoints = new System.Windows.Media.PointCollection();
                    foreach (var p in points)
                    {
                        double x = (p.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05);
                        double y = MapCanvasSize - ((p.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05));
                        projPoints.Add(new System.Windows.Point(x, y));
                    }

                    string color = "#FFFFFF";
                    if (lap == ReferenceLap)
                    {
                        color = RefColor;
                    }
                    else if (lap == SelectedLap)
                    {
                        color = SpeedColor;
                    }
                    else
                    {
                        var lapLabel = $"T{lap.Number}";
                        var legend = LegendEntries.FirstOrDefault(le => le.Label != null && le.Label.Contains(lapLabel));
                        color = legend?.Color ?? "#6366f1";
                    }

                    LapTrajectories.Add(new LapTrajectory
                    {
                        Color = color,
                        Points = projPoints,
                        Thickness = lap == SelectedLap ? dynamicThickness : (dynamicThickness * 0.8),
                        Opacity = 1.0f
                    });
                }
            }

            // La projection a pu changer : replacer les marqueurs Vmin
            ProjectCornerMarkers();
        }

        private System.Windows.Point ProjectToMap(TelemetryPoint p)
        {
            double x = (p.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05);
            double y = MapCanvasSize - ((p.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05));
            return new System.Windows.Point(x, y);
        }
        private string GetColorForAccel(double accel, double limitPos, double limitNeg)
        {
            // Accel > 0 dans le log = Freinage (Rouge)
            // Accel < 0 dans le log = Accélération (Vert)
            
            string neutral = "#94a3b8"; // Gris bleuâtre
            string green = "#39FF14";   // Neon Green Fluo
            string red = "#FF3131";     // Neon Red Fluo
            
            if (accel < 0) // Accélération
            {
                double val = Math.Clamp(Math.Abs(accel) / limitNeg, 0, 1.0);
                return LerpColor(neutral, green, val);
            }
            else // Freinage
            {
                double val = Math.Clamp(accel / limitPos, 0, 1.0);
                return LerpColor(neutral, red, val);
            }
        }

        private string LerpColor(string colorStart, string colorEnd, double amount)
        {
            var start = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorStart);
            var end = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorEnd);
            
            byte r = (byte)(start.R + (end.R - start.R) * amount);
            byte g = (byte)(start.G + (end.G - start.G) * amount);
            byte b = (byte)(start.B + (end.B - start.B) * amount);
            
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
