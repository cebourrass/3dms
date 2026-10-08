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
        private ISeries[] _telemetrySeries = Array.Empty<ISeries>();
        public ISeries[] TelemetrySeries
        {
            get => _telemetrySeries;
            set => SetProperty(ref _telemetrySeries, value);
        }

        private RectangularSection[] _sections = Array.Empty<RectangularSection>();
        public RectangularSection[] Sections
        {
            get => _sections;
            set => SetProperty(ref _sections, value);
        }

        // Paramètres de style (Onglet Paramètres)
        private string _speedColor = "#10b981"; // Emerald
        public string SpeedColor { get => _speedColor; set { if (SetProperty(ref _speedColor, value)) UpdateTelemetryCharts(); } }
        private float _speedThickness = 1.8f;
        public float SpeedThickness { get => _speedThickness; set { if (SetProperty(ref _speedThickness, value)) UpdateTelemetryCharts(); } }

        private string _angleColor = "#fbbf24"; // Amber
        public string AngleColor { get => _angleColor; set { if (SetProperty(ref _angleColor, value)) UpdateTelemetryCharts(); } }
        private float _angleThickness = 1.8f;
        public float AngleThickness { get => _angleThickness; set { if (SetProperty(ref _angleThickness, value)) UpdateTelemetryCharts(); } }

        private string _angleRightColor = "#f59e0b"; // Orange/Amber 600
        public string AngleRightColor { get => _angleRightColor; set { if (SetProperty(ref _angleRightColor, value)) UpdateTelemetryCharts(); } }
        private float _angleRightThickness = 1.8f;
        public float AngleRightThickness { get => _angleRightThickness; set { if (SetProperty(ref _angleRightThickness, value)) UpdateTelemetryCharts(); } }

        private string _accelColor = "#8b5cf6"; // Violet
        public string AccelColor { get => _accelColor; set { if (SetProperty(ref _accelColor, value)) UpdateTelemetryCharts(); } }
        private float _accelThickness = 1.8f;
        public float AccelThickness { get => _accelThickness; set { if (SetProperty(ref _accelThickness, value)) UpdateTelemetryCharts(); } }

        private string _decelColor = "#ef4444"; // Red 500
        public string DecelColor { get => _decelColor; set { if (SetProperty(ref _decelColor, value)) UpdateTelemetryCharts(); } }
        private float _decelThickness = 1.8f;
        public float DecelThickness { get => _decelThickness; set { if (SetProperty(ref _decelThickness, value)) UpdateTelemetryCharts(); } }

        private string _refColor = "#ffffff"; // White
        public string RefColor { get => _refColor; set { if (SetProperty(ref _refColor, value)) UpdateTelemetryCharts(); } }
        private float _refThickness = 1.2f;
        public float RefThickness { get => _refThickness; set { if (SetProperty(ref _refThickness, value)) UpdateTelemetryCharts(); } }

        private int _speedSmoothing;
        public int SpeedSmoothing { get => _speedSmoothing; set { if (SetProperty(ref _speedSmoothing, value)) UpdateTelemetryCharts(); } }

        private int _angleSmoothing;
        public int AngleSmoothing { get => _angleSmoothing; set { if (SetProperty(ref _angleSmoothing, value)) UpdateTelemetryCharts(); } }

        private int _accelSmoothing;
        public int AccelSmoothing { get => _accelSmoothing; set { if (SetProperty(ref _accelSmoothing, value)) UpdateTelemetryCharts(); } }

        private int _gpsSmoothing;
        public int GpsSmoothing { get => _gpsSmoothing; set { if (SetProperty(ref _gpsSmoothing, value)) UpdateTelemetryCharts(); } }

        public class SmoothingOption
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }

        public List<SmoothingOption> SmoothingOptions { get; } = new()
        {
            new SmoothingOption { Name = "Brut", Value = 1 },
            new SmoothingOption { Name = "Standard", Value = 3 },
            new SmoothingOption { Name = "Fort", Value = 5 },
            new SmoothingOption { Name = "Très fort", Value = 8 }
        };

        public class InterpolationOption
        {
            public string Name { get; set; } = string.Empty;
            public double StepMs { get; set; }
        }

        public List<InterpolationOption> InterpolationOptions { get; } = new()
        {
            new InterpolationOption { Name = "10 Hz (100ms)", StepMs = 100.0 },
            new InterpolationOption { Name = "25 Hz (40ms)", StepMs = 40.0 },
            new InterpolationOption { Name = "50 Hz (20ms)", StepMs = 20.0 },
            new InterpolationOption { Name = "100 Hz (10ms)", StepMs = 10.0 }
        };

        private double _interpolationStepMs;
        public double InterpolationStepMs { get => _interpolationStepMs; set { if (SetProperty(ref _interpolationStepMs, value)) UpdateTelemetryCharts(); } }

        private bool _showDeltaTime;
        public bool ShowDeltaTime { get => _showDeltaTime; set { if (SetProperty(ref _showDeltaTime, value)) UpdateTelemetryCharts(); } }


        // Comparison Thickness Range (Fast/Slow)
        private float _compFastThickness = 1.5f;
        public float CompFastThickness { get => _compFastThickness; set { if (SetProperty(ref _compFastThickness, value)) UpdateTelemetryCharts(); } }
        private float _compSlowThickness = 0.5f;
        public float CompSlowThickness { get => _compSlowThickness; set { if (SetProperty(ref _compSlowThickness, value)) UpdateTelemetryCharts(); } }

        private bool _showSpeed = true;
        public bool ShowSpeed { get => _showSpeed; set { if (SetProperty(ref _showSpeed, value)) UpdateTelemetryCharts(); } }
        
        private bool _showAngleLeft = true;
        public bool ShowAngleLeft { get => _showAngleLeft; set { if (SetProperty(ref _showAngleLeft, value)) { UpdateTelemetryCharts(); OnPropertyChanged(nameof(IsAnyAngleVisible)); } } }

        private bool _showAngleRight = true;
        public bool ShowAngleRight { get => _showAngleRight; set { if (SetProperty(ref _showAngleRight, value)) { UpdateTelemetryCharts(); OnPropertyChanged(nameof(IsAnyAngleVisible)); } } }
        
        private bool _showAccel = false;
        public bool ShowAccel { get => _showAccel; set { if (SetProperty(ref _showAccel, value)) { UpdateTelemetryCharts(); OnPropertyChanged(nameof(IsAnyAccelVisible)); } } }

        private bool _showDecel = false;
        public bool ShowDecel { get => _showDecel; set { if (SetProperty(ref _showDecel, value)) { UpdateTelemetryCharts(); OnPropertyChanged(nameof(IsAnyAccelVisible)); } } }

        public bool IsAnyAngleVisible => ShowAngleLeft || ShowAngleRight;
        public bool IsAnyAccelVisible => ShowAccel || ShowDecel;

        private double _currentX;
        public double CurrentX { get => _currentX; set => SetProperty(ref _currentX, value); }

        private TelemetryPoint? _currentTelemetryPoint;
        public TelemetryPoint? CurrentTelemetryPoint
        {
            get => _currentTelemetryPoint;
            set => SetProperty(ref _currentTelemetryPoint, value);
        }
        
        private double _currentSpeed;
        public double CurrentSpeed { get => _currentSpeed; set => SetProperty(ref _currentSpeed, value); }
        
        private double _currentDelta;
        public double CurrentDelta { get => _currentDelta; set => SetProperty(ref _currentDelta, value); }
        
        private double _currentAngle;
        public double CurrentAngle { get => _currentAngle; set => SetProperty(ref _currentAngle, value); }
        
        private float _currentAccel;
        public float CurrentAccel { get => _currentAccel; set => SetProperty(ref _currentAccel, value); }

        private string _currentAngleColor = "#fbbf24";
        public string CurrentAngleColor { get => _currentAngleColor; set => SetProperty(ref _currentAngleColor, value); }

        private string _currentAccelColor = "#8b5cf6";
        public string CurrentAccelColor { get => _currentAccelColor; set => SetProperty(ref _currentAccelColor, value); }
        
        public ObservableCollection<LegendEntry> LegendEntries { get; } = new();

        public SolidColorPaint LegendTextPaint { get; } = new SolidColorPaint(new SKColor(200, 200, 200));

        private List<TelemetryPoint> _currentLapPoints = new();
        private List<TelemetryPoint>? _interpolatedPoints;

        private string _xAxisValueLabel = "TEMPS";
        public string XAxisValueLabel => _xAxisValueLabel;
        public string XAxisUnit => _xAxisValueLabel == "DISTANCE" ? "m" : "s";

        public ObservableCollection<CursorLapValue> CursorLaps { get; } = new();

        public Axis[] XAxes { get; set; } = 
        {
            new Axis
            {
                Name = "Time (min:sec)", // Will be updated in UpdateTelemetryCharts
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                Labeler = value => TimeSpan.FromSeconds(value).ToString(@"mm\:ss"),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9
            }
        };

        public Axis[] YAxes { get; set; } = 
        {
            new Axis // Axe 0 (Gauche) : Vitesse
            {
                Name = "Speed (km/h)", // Will be updated in UpdateTelemetryCharts
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9,
                Labeler = value => Math.Round(value).ToString(),
                Position = LiveChartsCore.Measure.AxisPosition.Start,
                MinStep = 50,
                SeparatorsPaint = new SolidColorPaint(new SKColor(100, 116, 139, 40), 0.5f)
            },
            new Axis // Axe 1 (Droite) : Angle / G
            {
                Name = "Angle (°) / G",
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9,
                Labeler = value => Math.Round(value, 1).ToString(),
                Position = LiveChartsCore.Measure.AxisPosition.End,
                ShowSeparatorLines = false
            },
            new Axis // Axe 2 (Droite ext) : Delta Time
            {
                Name = "Δ Time (s)",
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9,
                Labeler = value => value >= 0 ? $"+{value:F2}s" : $"{value:F2}s",
                Position = LiveChartsCore.Measure.AxisPosition.End,
                ShowSeparatorLines = false,
                IsVisible = false
            }
        };

        private void AddLapSeries(List<ISeries> seriesList, LapData? lap, string? label, float thicknessOverride, SKColor? overrideColor, bool isGlobalRef = false)
        {
            if (lap == null && !isGlobalRef) return;

            List<TelemetryPoint> points;
            double lapStart = 0;
            double startDist = 0;

            // Cas de la référence globale (déjà normalisée)
            if (isGlobalRef && _cachedReferencePoints != null)
            {
                points = _cachedReferencePoints;
                lapStart = 0;
                startDist = 0;
            }
            else
            {
                if (lap == null) return;
                var start = lap.StartTimeMs;
                var end = start + lap.LapTimeMs + 300; // Marge de 300ms pour inclure le point de jonction
                points = CurrentSession!.AllPoints
                    .Where(p => p.Time >= start && p.Time <= end)
                    .OrderBy(p => p.Time)
                    .ToList();
                lapStart = start;
                startDist = lap.StartDistance;
            }

            if (!points.Any()) return;

            // --- Lissage et Interpolation ---
            // On interpole à 50Hz (20ms) pour une fluidité maximale
            var processedPoints = InterpolateAndSmooth(points);

            if (lap == SelectedLap) 
            {
                _currentLapPoints = points;
                _interpolatedPoints = processedPoints;
            }
            if (lap != null)
            {
                lap.TelemetryPoints = processedPoints; // Utiliser les points lissés/interpolés
            }

            bool useDistance = (ComparisonLaps.Count > 1 || (ShowReference && _cachedReferencePoints != null));
            bool legendAdded = false;
            float smoothness = 0.65f;

            if (ShowSpeed)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : SpeedThickness;
                seriesList.Add(new LineSeries<ObservablePoint>
                {
                    Values = processedPoints.Select(p => new ObservablePoint(
                        useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, 
                        (double)p.Speed)).ToArray(),
                    Name = !legendAdded ? label : null,
                    Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(SpeedColor), thickness),
                    GeometrySize = 0,
                    Fill = null,
                    LineSmoothness = smoothness,
                    ScalesYAt = 0 // Axe de gauche
                });
                legendAdded = true;
            }

            if (ShowAngleLeft || ShowAngleRight)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : AngleThickness;
                float thicknessRight = thicknessOverride > 0 ? thicknessOverride : AngleRightThickness;

                if (ShowAngleLeft)
                {
                    // Courbe Angle GAUCHE (Points négatifs mis en positif)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.LeanAngle <= 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)-p.LeanAngle)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = !legendAdded ? (label != null ? $"{label} (G)" : "Angle (G)") : null,
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AngleColor), thickness),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness,
                        ScalesYAt = 1 // Axe de droite
                    });
                }

                if (ShowAngleRight)
                {
                    // Courbe Angle DROIT (Points positifs)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.LeanAngle > 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)p.LeanAngle)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = label != null ? $"{label} (D)" : "Angle (D)",
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AngleRightColor), thicknessRight),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness,
                        ScalesYAt = 1 // Axe de droite
                    });
                }
                legendAdded = true;
            }

            if (ShowAccel || ShowDecel)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : AccelThickness;
                float thicknessDecel = thicknessOverride > 0 ? thicknessOverride : DecelThickness;

                if (ShowAccel)
                {
                    // Courbe Accélération (Points NÉGATIFS dans le log mis en positif)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.Acceleration < 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)-p.Acceleration * 40)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = !legendAdded ? (label != null ? $"{label} (Acc)" : "Accel") : null,
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AccelColor), thickness),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness,
                        ScalesYAt = 1 // Axe de droite
                    });
                }

                if (ShowDecel)
                {
                    // Courbe Décélération (Points POSITIFS dans le log)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.Acceleration >= 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)p.Acceleration * 40)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = label != null ? $"{label} (Frein)" : "Frein",
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(DecelColor), thicknessDecel),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness,
                        ScalesYAt = 1 // Axe de droite
                    });
                }
            }
        }

        private List<TelemetryPoint> InterpolateAndSmooth(List<TelemetryPoint> rawPoints)
        {
            if (rawPoints.Count < 2) return rawPoints;

            var smoothed = new List<TelemetryPoint>();
            
            for (int i = 0; i < rawPoints.Count; i++)
            {
                var pt = new TelemetryPoint { Time = rawPoints[i].Time };
                
                // Lissage Vitesse
                int sWindow = Math.Max(1, SpeedSmoothing);
                int sStart = Math.Max(0, i - sWindow / 2);
                int sEnd = Math.Min(rawPoints.Count - 1, i + sWindow / 2);
                pt.Speed = (float)rawPoints.Skip(sStart).Take(sEnd - sStart + 1).Average(p => (double)p.Speed);
                
                // Lissage Angle
                int aWindow = Math.Max(1, AngleSmoothing);
                int aStart = Math.Max(0, i - aWindow / 2);
                int aEnd = Math.Min(rawPoints.Count - 1, i + aWindow / 2);
                pt.LeanAngle = (float)rawPoints.Skip(aStart).Take(aEnd - aStart + 1).Average(p => (double)p.LeanAngle);
                
                // Lissage Accélération
                int gWindow = Math.Max(1, AccelSmoothing);
                int gStart = Math.Max(0, i - gWindow / 2);
                int gEnd = Math.Min(rawPoints.Count - 1, i + gWindow / 2);
                pt.Acceleration = (float)rawPoints.Skip(gStart).Take(gEnd - gStart + 1).Average(p => (double)p.Acceleration);
                
                // Lissage Coordonnées (GPS Jitter)
                int cWindow = Math.Max(1, GpsSmoothing);
                int cStart = Math.Max(0, i - cWindow / 2);
                int cEnd = Math.Min(rawPoints.Count - 1, i + cWindow / 2);
                pt.Latitude = (float)rawPoints.Skip(cStart).Take(cEnd - cStart + 1).Average(p => (double)p.Latitude);
                pt.Longitude = (float)rawPoints.Skip(cStart).Take(cEnd - cStart + 1).Average(p => (double)p.Longitude);
                
                // Distance
                pt.Distance = rawPoints[i].Distance;

                smoothed.Add(pt);
            }

            var interpolated = new List<TelemetryPoint>();
            double startTime = rawPoints.First().Time;
            double endTime = rawPoints.Last().Time;
            double step = InterpolationStepMs;
            double duration = endTime - startTime;

            // Sécurité : si la durée est absurde (ex: > 30 min pour un tour), on limite l'interpolation
            if (duration > 1800000 || duration < 0) 
            {
                // Retourner les points lissés sans interpolation pour éviter le freeze/OOM
                return smoothed;
            }

            // Optimisation : utiliser un index pour éviter de reparcourir la liste (O(N) -> O(1) par itération)
            int p1Idx = 0;
            for (double t = startTime; t <= endTime; t += step)
            {
                // Trouver le segment [p1, p2] contenant t
                while (p1Idx < smoothed.Count - 1 && smoothed[p1Idx + 1].Time <= t)
                {
                    p1Idx++;
                }

                if (p1Idx < smoothed.Count - 1)
                {
                    var p1 = smoothed[p1Idx];
                    var p2 = smoothed[p1Idx + 1];
                    
                    double timeGap = p2.Time - p1.Time;
                    if (timeGap > 0)
                    {
                        float ratio = (float)((t - p1.Time) / timeGap);
                        interpolated.Add(new TelemetryPoint
                        {
                            Time = (uint)t,
                            Distance = p1.Distance + (p2.Distance - p1.Distance) * ratio,
                            Speed = p1.Speed + (p2.Speed - p1.Speed) * ratio,
                            LeanAngle = p1.LeanAngle + (p2.LeanAngle - p1.LeanAngle) * ratio,
                            Acceleration = p1.Acceleration + (p2.Acceleration - p1.Acceleration) * ratio,
                            Latitude = p1.Latitude + (p2.Latitude - p1.Latitude) * ratio,
                            Longitude = p1.Longitude + (p2.Longitude - p1.Longitude) * ratio
                        });
                    }
                    else
                    {
                        interpolated.Add(p1);
                    }
                }
                else
                {
                    interpolated.Add(smoothed.Last());
                }
            }

            // Toujours s'assurer que le dernier point exact est ajouté pour fermer le tracé
            if (interpolated.Last().Time < smoothed.Last().Time)
            {
                interpolated.Add(smoothed.Last());
            }

            // Sécurité supplémentaire : limiter le nombre de points interpolés (max 200,000)
            if (interpolated.Count > 200000) return interpolated;

            return interpolated;
        }

        public void UpdateTelemetryCharts()
        {
            if (SelectedLap == null || CurrentSession == null)
            {
                TelemetrySeries = Array.Empty<ISeries>();
                LegendEntries.Clear();
                return;
            }

            bool useDistance = (ComparisonLaps.Count > 1 || (ShowReference && ReferenceLap != null));
            var app = System.Windows.Application.Current;
            if (app == null) return;

            // Mise à jour de l'axe X
            if (useDistance)
            {
                XAxes[0].Name = "Distance (m)";
                XAxes[0].Labeler = value => $"{value:F0}";
            }
            else
            {
                XAxes[0].Name = (string)app.FindResource("AxisTime");
                XAxes[0].Labeler = value => TimeSpan.FromSeconds(value).ToString(@"mm\:ss");
            }
            
            YAxes[0].Name = (string)app.FindResource("AxisSpeed");
            YAxes[1].Name = (string)app.FindResource("AxisAngle");
            YAxes[2].Name = (string)app.FindResource("AxisDelta");
            OnPropertyChanged(nameof(XAxisValueLabel));

            var seriesList = new List<ISeries>();
            LegendEntries.Clear();

            // 3. Tours de Comparaison
            var comparePool = ComparisonLaps.Where(l => l.LapTimeMs > 0 && l != SelectedLap && l != ReferenceLap).ToList();
            bool isComparing = comparePool.Any();

            // Calculer les bornes globales sur TOUS les tours affichés pour une échelle cohérente
            var allVisible = new List<LapData?>();
            if (SelectedLap != null) allVisible.Add(SelectedLap);
            if (ShowReference && _cachedReferencePoints != null) allVisible.Add(null); // 'null' représentera la réf globale dans les boucles
            foreach (var l in comparePool) allVisible.Add(l);
            
            if (!allVisible.Any() || SelectedLap == null) return;
            
            double globalMinMs = allVisible.Min(l => l == null ? (ReferenceLap?.LapTimeMs ?? 0) : l.LapTimeMs);
            double globalMaxMs = allVisible.Max(l => l == null ? (ReferenceLap?.LapTimeMs ?? 0) : l.LapTimeMs);
            double globalRange = globalMaxMs - globalMinMs;

            int comparisonIndex = 0;
            var comparisonColors = new[] { "#6366f1", "#06b6d4", "#eab308", "#ef4444", "#a855f7" }; // SlateBlue, Cyan, Yellow, Red, Purple
            
            foreach (var lap in comparePool)
            {
                // Interpolation basée sur la plage GLOBALE
                double factor = globalRange > 0 ? (lap.LapTimeMs - globalMinMs) / globalRange : 0;
                float thickness = (float)(CompFastThickness + (factor * (CompSlowThickness - CompFastThickness)));
                
                string hexColor = comparisonColors[comparisonIndex % comparisonColors.Length];
                
                var color = SKColor.Parse(hexColor).WithAlpha(180);
                AddLapSeries(seriesList, lap, null, thickness, color);
                
                LegendEntries.Add(new LegendEntry { 
                    Label = $"T{lap.Number}", 
                    LapTime = lap.LapTime, 
                    Color = hexColor, 
                    Thickness = thickness,
                    SortTimeMs = lap.LapTimeMs
                });
                
                comparisonIndex++;
            }

            // 1. Tour Sélectionné (Vert) - Calcul d'épaisseur dynamique
            string selectedTime = SelectedLap.LapTime;
            string selectedLabel = (ShowReference && SelectedLap == ReferenceLap) ? $"[REF] T{SelectedLap.Number}" : $"T{SelectedLap.Number}";
            
            double sFactor = globalRange > 0 ? (SelectedLap.LapTimeMs - globalMinMs) / globalRange : 0;
            float sThickness = isComparing ? (float)(CompFastThickness + (sFactor * (CompSlowThickness - CompFastThickness))) : -1;
            
            // En solo, on utilise les couleurs individuelles (colorOverride = null)
            // En comparaison, on utilise le style de référence (si c'est le tour de référence)
            SKColor? sColorOverride = (isComparing && SelectedLap == ReferenceLap) ? SKColor.Parse(RefColor) : null;

            AddLapSeries(seriesList, SelectedLap, null, sThickness, sColorOverride); 
            LegendEntries.Add(new LegendEntry { 
                Label = selectedLabel, 
                LapTime = selectedTime, 
                Color = SpeedColor, 
                Thickness = sThickness,
                SortTimeMs = SelectedLap.LapTimeMs
            });

            // 2. Référence Globale - Calcul d'épaisseur dynamique
            if (ShowReference && _cachedReferencePoints != null)
            {
                double refMs = _cachedReferencePoints.Count > 0 ? (_cachedReferencePoints.Last().Time) : 0; 
                // Note: La comparaison de temps se fait sur LapTimeMs s'il y a un objet ReferenceLap, sinon on estime
                double lapDuration = ReferenceLap?.LapTimeMs ?? refMs;
                
                // La référence utilise toujours RefColor et RefThickness pour se distinguer
                float rThickness = (float)RefThickness;
                SKColor rColorOverride = SKColor.Parse(RefColor);
                
                AddLapSeries(seriesList, null, null, rThickness, rColorOverride, true);
                
                // Libellé propre
                LegendEntries.Add(new LegendEntry { 
                    Label = "[REF]", 
                    LapTime = _cachedReferenceTime, 
                    Color = RefColor, 
                    Thickness = (double)rThickness, 
                    IsReference = true,
                    SortTimeMs = ReferenceLap?.LapTimeMs ?? 0 
                });
            }

            // --- TRI DE LA LÉGENDE (Plus rapide au plus lent) ---
            var sortedList = LegendEntries.OrderBy(e => e.SortTimeMs).ToList();
            LegendEntries.Clear();
            foreach (var entry in sortedList) LegendEntries.Add(entry);

            TelemetrySeries = seriesList.ToArray();

            // Création des sections (barres verticales pour les partiels du tour ACTUEL)
            var sectionsList = new List<RectangularSection>();
            if (SelectedLap.PartialDistances != null)
            {
                // Barre de début de tour (T=0 / D=0)
                sectionsList.Add(new RectangularSection
                {
                    Xi = 0, Xj = 0,
                    Stroke = new SolidColorPaint(SKColors.White.WithAlpha(60), 1)
                });

                for (int i = 0; i < SelectedLap.PartialDistances.Length; i++)
                {
                    double pos = useDistance 
                        ? SelectedLap.PartialDistances[i] 
                        : SelectedLap.CumulativePartialTimesMs[i] / 1000.0;

                    if (pos > 0)
                    {
                        var section = new RectangularSection
                        {
                            Xi = pos, Xj = pos,
                            Stroke = new SolidColorPaint(SKColors.White.WithAlpha(40), 1)
                        };

                        if (i < SelectedLap.PartialDistances.Length - 1)
                        {
                            section.Label = $"P{i + 1}";
                            section.LabelPaint = new SolidColorPaint(new SKColor(148, 163, 184));
                            section.LabelSize = 11;
                        }
                        
                        sectionsList.Add(section);
                    }
                }
            }

            // Quadrillage dynamique basé sur TOUS les tours visibles
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var lap in allVisible)
            {
                List<TelemetryPoint> points;
                // Si c'est la référence globale, on utilise le cache VM
                if (lap == null && _cachedReferencePoints != null)
                {
                    points = _cachedReferencePoints;
                }
                else
                {
                    if (lap == null) continue;
                    var start = lap.StartTimeMs;
                    var end = start + lap.LapTimeMs;
                    points = CurrentSession!.AllPoints.Where(p => p.Time >= start && p.Time <= end).ToList();
                }

                if (!points.Any()) continue;

                if (ShowSpeed) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)p.Speed)); 
                    maxY = Math.Max(maxY, points.Max(p => (double)p.Speed)); 
                }
                if (IsAnyAngleVisible) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)Math.Abs(p.LeanAngle))); 
                    maxY = Math.Max(maxY, points.Max(p => (double)Math.Abs(p.LeanAngle))); 
                }
                if (IsAnyAccelVisible) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)Math.Abs(p.Acceleration) * 50)); 
                    maxY = Math.Max(maxY, points.Max(p => (double)Math.Abs(p.Acceleration) * 50)); 
                }
            }

            // Adaptation dynamique du titre de l'axe Y
            var activeSeries = new List<string>();
            if (ShowSpeed) activeSeries.Add("Vitesse");
            if (IsAnyAngleVisible) activeSeries.Add("Angle");
            if (IsAnyAccelVisible) activeSeries.Add("G");
            
            if (ShowDeltaTime)
            {
                // MODE DELTA TIME (Superposition sur l'axe de droite ext)
                UpdateDeltaView(seriesList);
                
                YAxes[2].IsVisible = true;
                
                // Ligne de référence à zéro pour le Delta
                sectionsList.Add(new RectangularSection
                {
                    Yi = 0, Yj = 0,
                    Stroke = new SolidColorPaint(SKColors.White.WithAlpha(100), 1.5f),
                    ScalesYAt = 2
                });

                // Calcul des bornes du Delta pour une graduation propre
                var deltaSeries = seriesList.OfType<LineSeries<ObservablePoint>>().FirstOrDefault(s => s.Name != null && s.Name.StartsWith("Delta"));
                if (deltaSeries != null && deltaSeries.Values != null && deltaSeries.Values.Any())
                {
                    double minD = deltaSeries.Values.Min(p => p.Y ?? 0);
                    double maxD = deltaSeries.Values.Max(p => p.Y ?? 0);
                    
                    // On arrondit pour avoir des graduations propres (ex: 0.5s)
                    double range = maxD - minD;
                    if (range < 0.1) range = 0.1; // Minimum de 0.1s de plage
                    
                    double margin = range * 0.1;
                    YAxes[2].MaxLimit = maxD + margin;
                    YAxes[2].MinLimit = minD - margin;
                }
                else
                {
                    YAxes[2].MinLimit = null;
                    YAxes[2].MaxLimit = null;
                }
            }
            else
            {
                YAxes[2].IsVisible = false;
            }

            Sections = sectionsList.ToArray();
            
            // Mise à jour de la carte / trajectoires
            UpdateTrajectoryUI(CurrentMap);

            // MODE TÉLÉMÉTRIE NORMAL (Axe de gauche)
            YAxes[0].Name = "Vitesse (km/h)";
            YAxes[0].Labeler = value => Math.Round(value).ToString();
            
            // Quadrillage horizontal dynamique (Vitesse)
            if (seriesList.OfType<LineSeries<ObservablePoint>>().Any(ls => ls.ScalesYAt == 0))
            {
                double minY_val = double.MaxValue;
                double maxY_val = double.MinValue;
                bool hasLeftData = false;

                foreach (var s in seriesList.OfType<LineSeries<ObservablePoint>>().Where(ls => ls.ScalesYAt == 0))
                {
                    if (s.Values == null) continue;
                    foreach (var p in s.Values)
                    {
                        if (p.Y < minY_val) minY_val = p.Y.Value;
                        if (p.Y > maxY_val) maxY_val = p.Y.Value;
                        hasLeftData = true;
                    }
                }

                if (hasLeftData && maxY_val > minY_val)
                {
                    minY_val = Math.Max(0, Math.Floor(minY_val / 10) * 10);
                    maxY_val = Math.Ceiling(maxY_val / 10) * 10;
                    double range = maxY_val - minY_val;
                    double stepY = range / 4;
                    var separators = new List<double>();
                    for (int i = 0; i <= 4; i++)
                    {
                        double val = minY_val + (i * stepY);
                        separators.Add(val);
                        sectionsList.Add(new RectangularSection
                        {
                            Yi = val,
                            Yj = val,
                            Stroke = new SolidColorPaint(SKColors.White.WithAlpha(15), 0.5f)
                        });
                    }
                    YAxes[0].CustomSeparators = separators.ToArray();
                    YAxes[0].MinLimit = minY_val;
                    YAxes[0].MaxLimit = maxY_val;
                }
            }
            else
            {
                YAxes[0].MinLimit = null;
                YAxes[0].MaxLimit = null;
                YAxes[0].CustomSeparators = null;
            }

            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));

            // Quadrillage temporel / Distance (4 divisions verticales)
            double lapRange = 0;
            if (useDistance && _currentLapPoints.Any())
            {
                lapRange = (_currentLapPoints.Last().Distance - _currentLapPoints.First().Distance);
            }
            else if (SelectedLap != null)
            {
                lapRange = SelectedLap.LapTimeMs / 1000.0;
            }

            XAxes[0].MinLimit = 0;
            XAxes[0].MaxLimit = lapRange;

            if (lapRange > 0)
            {
                double stepX = lapRange / 4;
                for (int i = 1; i < 4; i++)
                {
                    double val = i * stepX;
                    sectionsList.Add(new RectangularSection
                    {
                        Xi = val,
                        Xj = val,
                        Stroke = new SolidColorPaint(SKColors.White.WithAlpha(20), 0.5f)
                    });
                }
            }

            // Ajout du curseur (barre verticale rouge mobile)
            sectionsList.Add(new RectangularSection
            {
                Xi = CurrentX,
                Xj = CurrentX,
                Stroke = new SolidColorPaint(new SKColor(239, 68, 68), 2) // Red 500
            });
            Sections = sectionsList.ToArray();
            UpdateCursor(CurrentX, true);

            // Calculer les statistiques de régularité
            UpdateRegularityStats();
            
            TelemetrySeries = seriesList.ToArray();
            AnalyzeCorners();
        }

        private void GetDeltaLaps(out LapData? lap1, out LapData? lap2)
        {
            lap1 = null;
            lap2 = null;
            var selectedLaps = ComparisonLaps.Where(l => l != null && l.LapTimeMs > 0).ToList();

            if (ShowReference && ReferenceLap != null)
            {
                lap1 = ReferenceLap;
                // On cherche un tour sélectionné qui n'est pas la référence
                lap2 = selectedLaps.FirstOrDefault(l => l != ReferenceLap);
                // Si rien d'autre n'est sélectionné, on compare au tour actif s'il est différent
                if (lap2 == null && SelectedLap != null && SelectedLap != ReferenceLap) 
                    lap2 = SelectedLap;
            }
            else if (selectedLaps.Count >= 2)
            {
                // Sans référence, on prend les deux plus rapides parmi la sélection
                // Le plus rapide devient la base (lap1), le second devient la cible (lap2)
                var sorted = selectedLaps.OrderBy(l => l.LapTimeMs).ToList();
                lap1 = sorted[0];
                lap2 = sorted[1];
            }
        }

        private void UpdateDeltaView(List<ISeries> seriesList)
        {
            GetDeltaLaps(out var lap1, out var lap2);
            if (lap1 == null || lap2 == null) return;

            // Déterminer la couleur de la courbe Delta basée sur le tour comparé (lap2)
            string colorHex = "#f59e0b"; // Orange par défaut
            var entry = LegendEntries.FirstOrDefault(e => e.Label == $"T{lap2.Number}" || e.Label == $"[REF] T{lap2.Number}");
            if (entry != null) colorHex = entry.Color;
            else if (lap2 == SelectedLap) colorHex = SpeedColor;

            var points1 = lap1.TelemetryPoints ?? GetLapPoints(lap1);
            var points2 = lap2.TelemetryPoints ?? GetLapPoints(lap2);

            if (points1 == null || points2 == null || points1.Count < 2 || points2.Count < 2) return;

            double dist1 = points1.Last().Distance - points1[0].Distance;
            double dist2 = points2.Last().Distance - points2[0].Distance;
            double maxDist = Math.Min(dist1, dist2);

            var deltaPoints = new List<ObservablePoint>();
            for (double d = 0; d <= maxDist; d += 5.0)
            {
                double t1 = GetTimeAtDistance(points1, d);
                double t2 = GetTimeAtDistance(points2, d);
                deltaPoints.Add(new ObservablePoint(d, t2 - t1));
            }

            seriesList.Add(new LineSeries<ObservablePoint>
            {
                Values = deltaPoints,
                Name = $"Delta (vs {lap2.Number})",
                Stroke = new SolidColorPaint(SKColor.Parse(colorHex), 2),
                GeometrySize = 0,
                Fill = new SolidColorPaint(SKColor.Parse(colorHex).WithAlpha(30)), // Zone sous la courbe plus discrète
                LineSmoothness = 0, // Pas de lissage sur le delta brut
                ScalesYAt = 2 // 3ème axe (Delta Time)
            });
        }

        private List<TelemetryPoint> GetLapPoints(LapData lap)
        {
            if (lap == null) return new List<TelemetryPoint>();
            return CurrentSession!.AllPoints
                .Where(p => p.Time >= lap.StartTimeMs && p.Time <= lap.StartTimeMs + lap.LapTimeMs + 300)
                .OrderBy(p => p.Time)
                .ToList();
        }

        private double GetTimeAtDistance(List<TelemetryPoint> points, double relativeDist)
        {
            if (points == null || points.Count < 2) return 0;
            double startDist = points[0].Distance;
            double targetDist = relativeDist + startDist;
            double startTime = points[0].Time;

            if (targetDist <= startDist) return 0;
            if (targetDist >= points.Last().Distance) return (points.Last().Time - startTime) / 1000.0;

            int low = 0;
            int high = points.Count - 1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                if (points[mid].Distance == targetDist)
                    return (points[mid].Time - startTime) / 1000.0;
                if (points[mid].Distance < targetDist)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            if (high < 0) high = 0;
            if (low >= points.Count) low = points.Count - 1;

            if (high == low) return (points[high].Time - startTime) / 1000.0;

            double d1 = points[high].Distance;
            double d2 = points[low].Distance;
            double distRange = d2 - d1;

            if (distRange <= 0) return (points[high].Time - startTime) / 1000.0;

            double fraction = (targetDist - d1) / distRange;
            double t1 = points[high].Time;
            double t2 = points[low].Time;

            return (t1 - startTime + (t2 - t1) * fraction) / 1000.0;
        }

        public void UpdateCursor(double timeOrDist, bool force = false)
        {
            if (!force && Math.Abs(CurrentX - timeOrDist) < 0.001) return; // Éviter les mises à jour inutiles si pas de mouvement réel

            CurrentX = timeOrDist;
            
            string newLabel = (ComparisonLaps.Count > 1 || (ShowReference && ReferenceLap != null)) ? "DISTANCE" : "TEMPS";
            if (newLabel != _xAxisValueLabel)
            {
                _xAxisValueLabel = newLabel;
                OnPropertyChanged(nameof(XAxisValueLabel));
                OnPropertyChanged(nameof(XAxisUnit));
            }

            bool useDistance = (newLabel == "DISTANCE");
            
            // 1. Mettre à jour les valeurs de base (SelectedLap)
            var pointsForCursor = _interpolatedPoints ?? _currentLapPoints;
            if (pointsForCursor != null && pointsForCursor.Any())
            {
                TelemetryPoint? point = null;
                if (useDistance)
                {
                    double startDist = pointsForCursor[0].Distance;
                    point = FindClosestPoint(pointsForCursor, timeOrDist, true, startDist);
                }
                else
                {
                    uint targetTime = (uint)(SelectedLap.StartTimeMs + (timeOrDist * 1000.0));
                    point = FindClosestPoint(pointsForCursor, targetTime, false, 0);
                }

                if (point != null)
                {
                    CurrentTelemetryPoint = point;
                    
                    // Projection du curseur sur la carte
                    if (_mapScale > 0)
                    {
                        CursorMapX = (point.Longitude - _mapMinLon) * _mapRatio * _mapScale + (MapCanvasSize * 0.05);
                        CursorMapY = MapCanvasSize - ((point.Latitude - _mapMinLat) * _mapScale + (MapCanvasSize * 0.05));

                        if (AutoCenterMap)
                        {
                            // Recentrer la carte sur le curseur
                            // MapPanX/Y sont appliqués à la Viewbox. On veut que CursorMapX/Y soit au centre du viewport.
                            // Le centre théorique est MapCanvasSize / 2 (250).
                            MapPanX = (MapCanvasSize / 2.0 - CursorMapX) * MapZoom;
                            MapPanY = (MapCanvasSize / 2.0 - CursorMapY) * MapZoom;
                        }
                    }
                    CurrentSpeed = point.Speed;
                    CurrentAngle = Math.Abs(point.LeanAngle);
                    CurrentAngleColor = point.LeanAngle <= 0 ? AngleColor : AngleRightColor;
                    CurrentAccel = Math.Abs(point.Acceleration);
                    CurrentAccelColor = point.Acceleration >= 0 ? AccelColor : DecelColor;
                }
            }

            // 2. Mettre à jour la position de la barre rouge (immédiat pour la fluidité)
            if (Sections != null && Sections.Length > 0)
            {
                var cursorSection = Sections.Last();
                cursorSection.Xi = timeOrDist;
                cursorSection.Xj = timeOrDist;
            }

            // --- DEBOUNCE POUR LE RESTE (Secondaire) ---
            if (_cursorDebounceTimer == null)
            {
                _cursorDebounceTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
                _cursorDebounceTimer.Tick += (s, e) =>
                {
                    _cursorDebounceTimer.Stop();
                    UpdateCursorSecondary(CurrentX, _xAxisValueLabel == "DISTANCE");
                };
            }
            _cursorDebounceTimer.Stop();
            _cursorDebounceTimer.Start();
        }

        private void UpdateCursorSecondary(double timeOrDist, bool useDistance)
        {
            // 3. Mettre à jour CursorLaps pour TOUS les tours comparés (Réutilisation des objets pour la fluidité)
            var allVisible = new List<LapData>();
            if (ShowReference && ReferenceLap != null) allVisible.Add(ReferenceLap);
            if (SelectedLap != null && !allVisible.Contains(SelectedLap)) allVisible.Add(SelectedLap);
            foreach (var lap in ComparisonLaps) if (!allVisible.Contains(lap)) allVisible.Add(lap);

            int index = 0;
            foreach (var lap in allVisible)
            {
                if (lap == null) continue;
                
                List<TelemetryPoint>? points = (lap == SelectedLap) ? _currentLapPoints 
                                             : (lap == ReferenceLap && _cachedReferencePoints != null) ? _cachedReferencePoints 
                                             : lap.TelemetryPoints;

                if (points == null || !points.Any()) continue;
                
                TelemetryPoint? p = null;
                if (useDistance)
                {
                    double lapStartDist = (lap == ReferenceLap && points == _cachedReferencePoints) ? 0 : points[0].Distance;
                    p = FindClosestPoint(points, timeOrDist, true, lapStartDist);
                }
                else
                {
                    double targetTime = (lap == ReferenceLap && points == _cachedReferencePoints) ? (timeOrDist * 1000.0) : (lap.StartTimeMs + (timeOrDist * 1000.0));
                    p = FindClosestPoint(points, targetTime, false, 0);
                }

                if (p != null)
                {
                    string color = lap == ReferenceLap ? RefColor : (lap == SelectedLap ? SpeedColor : "#6366f1");
                    
                    var legend = LegendEntries.FirstOrDefault(le => le.Label.Contains(lap.LapTime));
                    if (legend != null && legend.Color != null && lap != SelectedLap && lap != ReferenceLap) color = legend.Color;

                    if (index < CursorLaps.Count)
                    {
                        var item = CursorLaps[index];
                        item.LapName = lap == ReferenceLap ? "RÉF" : $"T{lap.Number}";
                        item.LapTime = lap.LapTime;
                        item.Color = color;
                        item.Speed = p.Speed;
                        item.Angle = Math.Abs(p.LeanAngle);
                        item.Accel = p.Acceleration;
                    }
                    else
                    {
                        CursorLaps.Add(new CursorLapValue
                        {
                            LapName = lap == ReferenceLap ? "RÉF" : $"T{lap.Number}",
                            LapTime = lap.LapTime,
                            Color = color,
                            Speed = p.Speed,
                            Angle = Math.Abs(p.LeanAngle),
                            Accel = p.Acceleration
                        });
                    }
                    index++;
                }
            }
            while (CursorLaps.Count > index) CursorLaps.RemoveAt(CursorLaps.Count - 1);

            // 4. Mettre à jour le Delta en temps réel (si activé)
            if (useDistance && ShowDeltaTime)
            {
                GetDeltaLaps(out var lap1, out var lap2);
                if (lap1 != null && lap2 != null)
                {
                    var pts1 = lap1.TelemetryPoints ?? GetLapPoints(lap1);
                    var pts2 = lap2.TelemetryPoints ?? GetLapPoints(lap2);

                    if (pts1 != null && pts2 != null && pts1.Count > 1 && pts2.Count > 1)
                    {
                        double dist1 = pts1.Last().Distance - pts1[0].Distance;
                        double dist2 = pts2.Last().Distance - pts2[0].Distance;
                        double maxD = Math.Min(dist1, dist2);

                        if (timeOrDist <= maxD)
                        {
                            double t1 = GetTimeAtDistance(pts1, timeOrDist);
                            double t2 = GetTimeAtDistance(pts2, timeOrDist);
                            CurrentDelta = t2 - t1;
                        }
                        else CurrentDelta = 0;
                    }
                    else CurrentDelta = 0;
                }
                else CurrentDelta = 0;
            }
            else CurrentDelta = 0;
        }

        private TelemetryPoint? FindClosestPoint(List<TelemetryPoint> points, double target, bool useDistance, double startDist = 0)
        {
            if (points == null || points.Count == 0) return null;
            
            int low = 0;
            int high = points.Count - 1;
            
            while (low <= high)
            {
                int mid = (low + high) / 2;
                double val = useDistance ? (points[mid].Distance - startDist) : points[mid].Time;
                
                if (val < target) low = mid + 1;
                else if (val > target) high = mid - 1;
                else return points[mid];
            }
            
            if (low >= points.Count) return points[points.Count - 1];
            if (high < 0) return points[0];
            
            double valLow = useDistance ? (points[low].Distance - startDist) : points[low].Time;
            double valHigh = useDistance ? (points[high].Distance - startDist) : points[high].Time;
            
            return (Math.Abs(valLow - target) < Math.Abs(valHigh - target)) ? points[low] : points[high];
        }
    }
}
