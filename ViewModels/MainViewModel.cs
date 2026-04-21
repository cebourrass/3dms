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
    public class LegendEntry : ObservableObject
    {
        public string Label { get; set; } = string.Empty;
        public string LapTime { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF";
        public double Thickness { get; set; } = 1.5;
        public bool IsReference { get; set; }
    }

    public class CircuitMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public GpsPoint? StartPoint { get; set; }
        public override string ToString() => Name;
    }
    public partial class MainViewModel : ObservableObject
    {
        private readonly Ra1ReaderService _readerService = new Ra1ReaderService();
        private readonly MapReaderService _mapReaderService = new MapReaderService();
        private readonly LapService _lapService = new LapService();

        private TrackMap? _currentMap;
        public TrackMap? CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

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

        private LapData? _referenceLap;
        public LapData? ReferenceLap
        {
            get => _referenceLap;
            set => SetProperty(ref _referenceLap, value);
        }

        private bool _showReference = true;
        public bool ShowReference { get => _showReference; set { if (SetProperty(ref _showReference, value)) UpdateTelemetryCharts(); } }

        public List<LapData> ComparisonLaps { get; set; } = new();

        // Paramètres de style (Onglet Paramètres)
        private string _speedColor = "#10b981"; // Emerald
        public string SpeedColor { get => _speedColor; set { if (SetProperty(ref _speedColor, value)) UpdateTelemetryCharts(); } }
        private float _speedThickness = 1.8f;
        public float SpeedThickness { get => _speedThickness; set { if (SetProperty(ref _speedThickness, value)) UpdateTelemetryCharts(); } }

        private string _angleColor = "#fbbf24"; // Amber
        public string AngleColor { get => _angleColor; set { if (SetProperty(ref _angleColor, value)) UpdateTelemetryCharts(); } }
        private float _angleThickness = 1.8f;
        public float AngleThickness { get => _angleThickness; set { if (SetProperty(ref _angleThickness, value)) UpdateTelemetryCharts(); } }

        private string _accelColor = "#8b5cf6"; // Violet
        public string AccelColor { get => _accelColor; set { if (SetProperty(ref _accelColor, value)) UpdateTelemetryCharts(); } }
        private float _accelThickness = 1.8f;
        public float AccelThickness { get => _accelThickness; set { if (SetProperty(ref _accelThickness, value)) UpdateTelemetryCharts(); } }

        private string _refColor = "#ffffff"; // White
        public string RefColor { get => _refColor; set { if (SetProperty(ref _refColor, value)) UpdateTelemetryCharts(); } }
        private float _refThickness = 1.2f;
        public float RefThickness { get => _refThickness; set { if (SetProperty(ref _refThickness, value)) UpdateTelemetryCharts(); } }

        // Comparison Thickness Range (Fast/Slow)
        private float _compFastThickness = 1.5f;
        public float CompFastThickness { get => _compFastThickness; set { if (SetProperty(ref _compFastThickness, value)) UpdateTelemetryCharts(); } }
        private float _compSlowThickness = 0.5f;
        public float CompSlowThickness { get => _compSlowThickness; set { if (SetProperty(ref _compSlowThickness, value)) UpdateTelemetryCharts(); } }

        private bool _showSpeed = true;
        public bool ShowSpeed { get => _showSpeed; set { if (SetProperty(ref _showSpeed, value)) UpdateTelemetryCharts(); } }
        
        private bool _showAngle = true;
        public bool ShowAngle { get => _showAngle; set { if (SetProperty(ref _showAngle, value)) UpdateTelemetryCharts(); } }
        
        private bool _showAccel = false;
        public bool ShowAccel { get => _showAccel; set { if (SetProperty(ref _showAccel, value)) UpdateTelemetryCharts(); } }

        private double _currentX;
        public double CurrentX { get => _currentX; set => SetProperty(ref _currentX, value); }
        
        private double _currentSpeed;
        public double CurrentSpeed { get => _currentSpeed; set => SetProperty(ref _currentSpeed, value); }
        
        private double _currentAngle;
        public double CurrentAngle { get => _currentAngle; set => SetProperty(ref _currentAngle, value); }
        
        private float _currentAccel;
        public float CurrentAccel { get => _currentAccel; set => SetProperty(ref _currentAccel, value); }

        public ObservableCollection<LegendEntry> LegendEntries { get; } = new();

        public SolidColorPaint LegendTextPaint { get; } = new SolidColorPaint(new SKColor(200, 200, 200));

        private List<TelemetryPoint> _currentLapPoints = new();

        private LapData _selectedLap = null!;
        public LapData SelectedLap
        {
            get => _selectedLap;
            set
            {
                if (SetProperty(ref _selectedLap, value))
                {
                    UpdateTelemetryCharts();
                }
            }
        }

        private ExplorerItem _selectedExplorerItem = null!;
        public ExplorerItem SelectedExplorerItem
        {
            get => _selectedExplorerItem;
            set
            {
                if (SetProperty(ref _selectedExplorerItem, value) && value is SessionItem session)
                {
                    LoadSession(session.FilePath);
                }
            }
        }

        private ObservableCollection<LapData> _laps = new ObservableCollection<LapData>();
        public ObservableCollection<LapData> Laps
        {
            get => _laps;
            set => SetProperty(ref _laps, value);
        }

        private System.Windows.Media.PointCollection _trajectoryPoints = new System.Windows.Media.PointCollection();
        public System.Windows.Media.PointCollection TrajectoryPoints
        {
            get => _trajectoryPoints;
            set => SetProperty(ref _trajectoryPoints, value);
        }

        private string _circuitName = "Aucun circuit";
        public string CircuitName
        {
            get => _circuitName;
            set => SetProperty(ref _circuitName, value);
        }

        private ObservableCollection<CircuitMetadata> _availableCircuits = new();
        public ObservableCollection<CircuitMetadata> AvailableCircuits => _availableCircuits;

        private CircuitMetadata? _selectedCircuit;
        public CircuitMetadata? SelectedCircuit
        {
            get => _selectedCircuit;
            set
            {
                if (SetProperty(ref _selectedCircuit, value) && value != null && CurrentSession != null)
                {
                    ApplyCircuit(value.FilePath);
                }
            }
        }

        private ObservableCollection<string> _pilots = new ObservableCollection<string> { "Cédric Bourrassier", "Invité" };
        public ObservableCollection<string> Pilots => _pilots;

        private ObservableCollection<string> _trackConditionsList = new ObservableCollection<string> { "Dry", "Wet", "Damp", "Mixed" };
        public ObservableCollection<string> TrackConditionsList => _trackConditionsList;

        public string? SessionEvent
        {
            get => CurrentSession?.Event;
            set { if (CurrentSession != null) { CurrentSession.Event = value ?? ""; OnPropertyChanged(); } }
        }

        public string? SessionPilot
        {
            get => CurrentSession?.Pilot;
            set { if (CurrentSession != null) { CurrentSession.Pilot = value ?? ""; OnPropertyChanged(); } }
        }

        public string? SessionVehicle
        {
            get => CurrentSession?.Vehicle;
            set { if (CurrentSession != null) { CurrentSession.Vehicle = value ?? ""; OnPropertyChanged(); } }
        }

        public string? SessionTrackConditions
        {
            get => CurrentSession?.TrackConditions;
            set { if (CurrentSession != null) { CurrentSession.TrackConditions = value ?? "Dry"; OnPropertyChanged(); } }
        }

        public double SessionTrackTemperature
        {
            get => CurrentSession?.TrackTemperature ?? 20;
            set { if (CurrentSession != null) { CurrentSession.TrackTemperature = value; OnPropertyChanged(); } }
        }

        public string? SessionTires
        {
            get => CurrentSession?.Tires;
            set { if (CurrentSession != null) { CurrentSession.Tires = value ?? ""; OnPropertyChanged(); } }
        }

        public string? SessionNotes
        {
            get => CurrentSession?.Notes;
            set { if (CurrentSession != null) { CurrentSession.Notes = value ?? ""; OnPropertyChanged(); } }
        }

        private ObservableCollection<ExplorerItem> _explorerItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get => _explorerItems;
            set => SetProperty(ref _explorerItems, value);
        }

        public Axis[] XAxes { get; set; } = 
        {
            new Axis
            {
                Name = "Temps (min:sec)",
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                Labeler = value => TimeSpan.FromSeconds(value).ToString(@"mm\:ss"),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9
            }
        };

        public Axis[] YAxes { get; set; } = 
        {
            new Axis
            {
                Name = "Données",
                NamePaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                TextSize = 9,
                Labeler = value => Math.Round(value).ToString(),
                ShowSeparatorLines = false
            }
        };

        private bool _isLapsVisible = true;
        public bool IsLapsVisible
        {
            get => _isLapsVisible;
            set => SetProperty(ref _isLapsVisible, value);
        }

        private bool _isMapVisible = true;
        public bool IsMapVisible
        {
            get => _isMapVisible;
            set => SetProperty(ref _isMapVisible, value);
        }

        private bool _isSessionInfoVisible = true;
        public bool IsSessionInfoVisible
        {
            get => _isSessionInfoVisible;
            set => SetProperty(ref _isSessionInfoVisible, value);
        }

        private bool _isChartsVisible = true;
        public bool IsChartsVisible
        {
            get => _isChartsVisible;
            set => SetProperty(ref _isChartsVisible, value);
        }

        public MainViewModel()
        {
            LoadAvailableCircuits();
            LoadExplorer();
            LoadDummyLaps();

            // Charge le premier fichier trouvé par défaut
            var firstSession = FindFirstSession(ExplorerItems);
            if (firstSession != null)
            {
                LoadSession(firstSession.FilePath);
            }
            else
            {
                LoadMockupData();
            }
        }

        private void LoadExplorer()
        {
            string rootPath = @"C:\dev\3DMS-CED";
            var rootFolder = new FolderItem { Name = "Ma Moto (3DMS Evo)" };
            
            // On cherche le dossier spécifique s'il existe
            string dataPath = System.IO.Path.Combine(rootPath, "3DMS Evo (38.39.8F.DC.D1.31)");
            if (System.IO.Directory.Exists(dataPath))
            {
                foreach (var dir in System.IO.Directory.GetDirectories(dataPath))
                {
                    var trackFolder = new FolderItem { Name = System.IO.Path.GetFileName(dir) };
                    foreach (var file in System.IO.Directory.GetFiles(dir, "*.ra1"))
                    {
                        trackFolder.Children.Add(new SessionItem { Name = System.IO.Path.GetFileName(file), FilePath = file });
                    }
                    rootFolder.Children.Add(trackFolder);
                }
            }
            
            ExplorerItems.Add(rootFolder);
        }

        private void LoadAvailableCircuits()
        {
            _availableCircuits.Clear();
            string mapsRoot = @"C:\dev\3DMS-CED\Map\Circuits";
            if (!Directory.Exists(mapsRoot)) return;

            foreach (var countryDir in Directory.GetDirectories(mapsRoot))
            {
                foreach (var mapFile in Directory.GetFiles(countryDir, "*.map"))
                {
                    try
                    {
                        // Optimization: just read enough to get the Start marker
                        var map = _mapReaderService.ReadMap(mapFile);
                        var startMarker = map.Markers.FirstOrDefault(m => m.Key.StartsWith("Start", StringComparison.OrdinalIgnoreCase)).Value;
                        
                        _availableCircuits.Add(new CircuitMetadata 
                        { 
                            Name = map.Name, 
                            FilePath = mapFile,
                            StartPoint = startMarker
                        });
                    }
                    catch { /* Ignore corrupted map files */ }
                }
            }
            // Sort by name
            var sorted = _availableCircuits.OrderBy(c => c.Name).ToList();
            _availableCircuits.Clear();
            foreach (var c in sorted) _availableCircuits.Add(c);
        }

        private CircuitMetadata? DetectCircuit(List<TelemetryPoint> points)
        {
            if (!points.Any() || !_availableCircuits.Any()) return null;

            var firstPoint = points[0];
            CircuitMetadata? bestMatch = null;
            double minDistance = double.MaxValue;

            foreach (var circuit in _availableCircuits)
            {
                if (circuit.StartPoint == null) continue;

                double dist = CalculateDistance(firstPoint.Latitude, firstPoint.Longitude, 
                                                circuit.StartPoint.Latitude, circuit.StartPoint.Longitude);
                
                // If within 5km, it's a good candidate
                if (dist < 5000 && dist < minDistance)
                {
                    minDistance = dist;
                    bestMatch = circuit;
                }
            }

            return bestMatch;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371.0 * c * 1000.0; // Meters
        }

        private SessionItem? FindFirstSession(IEnumerable<ExplorerItem> items)
        {
            foreach (var item in items)
            {
                if (item is SessionItem session) return session;
                if (item is FolderItem folder)
                {
                    var found = FindFirstSession(folder.Children);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void LoadDummyLaps()
        {
            Laps.Clear();
            Laps.Add(new LapData { Number = 1, Type = "Incomplet", CumulativeTime = "00:32.72", MaxSpeed = 44, MinSpeed = 0, MaxLeanLeft = 3, MaxLeanRight = 17, MaxAccel = 0.17f, MaxDecel = 0.09f, Partials = new[] { "-", "-", "-" } });
            Laps.Add(new LapData { Number = 2, Type = "Incomplet", CumulativeTime = "00:31.62", MaxSpeed = 37, MinSpeed = 31, MaxLeanLeft = 1, MaxLeanRight = 2, MaxAccel = 0.00f, MaxDecel = 0.11f, Partials = new[] { "-", "-", "-" } });
            Laps.Add(new LapData { Number = 3, Type = "Complet", CumulativeTime = "03:11.83", LapTime = "02:40.21", MaxSpeed = 184, MinSpeed = 0, MaxLeanLeft = 29, MaxLeanRight = 43, MaxAccel = 0.29f, MaxDecel = 0.22f, Partials = new[] { "01:10.96", "00:44.32", "00:26.33" } });
            Laps.Add(new LapData { Number = 8, Type = "Complet", CumulativeTime = "12:44.79", LapTime = "01:50.96", MaxSpeed = 206, MinSpeed = 53, MaxLeanLeft = 30, MaxLeanRight = 26, MaxAccel = 0.29f, MaxDecel = 0.31f, Partials = new[] { "00:35.36", "00:34.93", "00:24.17" } });
        }

        private string _sessionTitle = "Aucune session";
        public string SessionTitle
        {
            get => _sessionTitle;
            set => SetProperty(ref _sessionTitle, value);
        }

        private SessionData? _currentSession;
        public SessionData? CurrentSession
        {
            get => _currentSession;
            set 
            {
                if (SetProperty(ref _currentSession, value))
                {
                    OnPropertyChanged(nameof(IsP1Visible));
                    OnPropertyChanged(nameof(IsP2Visible));
                    OnPropertyChanged(nameof(IsP3Visible));
                    OnPropertyChanged(nameof(IsP4Visible));
                    
                    OnPropertyChanged(nameof(SessionEvent));
                    OnPropertyChanged(nameof(SessionPilot));
                    OnPropertyChanged(nameof(SessionVehicle));
                    OnPropertyChanged(nameof(SessionTrackConditions));
                    OnPropertyChanged(nameof(SessionTrackTemperature));
                    OnPropertyChanged(nameof(SessionTires));
                    OnPropertyChanged(nameof(SessionNotes));
                }
            }
        }

        public bool IsP1Visible => CurrentSession?.PartialCount >= 1;
        public bool IsP2Visible => CurrentSession?.PartialCount >= 2;
        public bool IsP3Visible => CurrentSession?.PartialCount >= 3;
        public bool IsP4Visible => CurrentSession?.PartialCount >= 4;

        private string _bestLapTime = "--:--.--";
        public string BestLapTime
        {
            get => _bestLapTime;
            set => SetProperty(ref _bestLapTime, value);
        }

        private string _idealLapTime = "--:--.--";
        public string IdealLapTime
        {
            get => _idealLapTime;
            set => SetProperty(ref _idealLapTime, value);
        }

        [RelayCommand]
        public void SetReference()
        {
            if (SelectedLap == null) return;
            
            // Unset old reference formatting
            foreach (var l in Laps) l.IsReference = false;
            
            ReferenceLap = SelectedLap;
            ReferenceLap.IsReference = true;
            UpdateTelemetryCharts();
        }

        [RelayCommand]
        public void ClearReference()
        {
            if (ReferenceLap != null) ReferenceLap.IsReference = false;
            ReferenceLap = null;
            UpdateTelemetryCharts();
        }

        [RelayCommand]
        public void LoadSession(string filePath)
        {
            var points = _readerService.ReadFile(filePath);
            if (points == null || !points.Any()) return;

            // Clear session-specific state
            ReferenceLap = null;
            ComparisonLaps.Clear();
            _currentLapPoints.Clear();

            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var session = new SessionData
            {
                Title = fileName,
                FilePath = filePath,
                AllPoints = points,
                Date = ParseDateFromFileName(fileName)
            };

            SessionTitle = session.Title;
            CurrentSession = session; // Set early so ApplyCircuit can use it

            // Circuit Detection
            var detected = DetectCircuit(points);
            if (detected != null)
            {
                _selectedCircuit = detected; // Use field to avoid re-triggering ApplyCircuit twice
                OnPropertyChanged(nameof(SelectedCircuit));
                ApplyCircuit(detected.FilePath);
            }
            else
            {
                // Fallback to name detection if GPS fails or as secondary check
                string? mapFile = FindMatchingMap(filePath);
                if (mapFile != null)
                {
                    SelectedCircuit = AvailableCircuits.FirstOrDefault(c => c.FilePath == mapFile);
                }
                else
                {
                    CircuitName = "Circuit inconnu";
                    TrajectoryPoints = new System.Windows.Media.PointCollection();
                }
            }
        }

        private void ApplyCircuit(string mapFilePath)
        {
            if (CurrentSession == null) return;

            CurrentSession.MapFilePath = mapFilePath;
            CurrentSession.CircuitMap = _mapReaderService.ReadMap(mapFilePath);
            
            int markerTimes = CurrentSession.CircuitMap.Markers.Keys.Count(k => k.StartsWith("Time", StringComparison.OrdinalIgnoreCase));
            CurrentSession.PartialCount = markerTimes > 0 ? markerTimes + 1 : 0;
            
            CurrentMap = CurrentSession.CircuitMap;
            CircuitName = CurrentMap.Name;
            UpdateTrajectoryUI(CurrentMap);

            RecalculateLaps();
        }

        private void RecalculateLaps()
        {
            if (CurrentSession == null || CurrentSession.CircuitMap == null) return;

            Laps.Clear();
            var calculatedLaps = _lapService.CalculateLaps(CurrentSession.AllPoints, CurrentSession.CircuitMap);
            
            if (calculatedLaps.Any())
            {
                var completeLaps = calculatedLaps.Where(l => l.Type == "Complet").ToList();
                if (completeLaps.Any())
                {
                    var best = completeLaps.OrderBy(l => l.LapTimeMs).First();
                    best.IsBestLap = true;
                    CurrentSession.BestLapTime = best.LapTime;
                    BestLapTime = best.LapTime;

                    double idealMs = 0;
                    int numSectors = CurrentSession.PartialCount;
                    if (numSectors > 0)
                    {
                        for (int s = 0; s < numSectors; s++)
                        {
                            var bestSectorMs = completeLaps
                                .Select(l => ParseTimeToMs(l.Partials[s]))
                                .Where(ms => ms > 0)
                                .DefaultIfEmpty(0)
                                .Min();
                            idealMs += bestSectorMs;
                        }
                        IdealLapTime = FormatTimeFromMs(idealMs);
                    }
                }

                CurrentSession.MaxSpeed = calculatedLaps.Max(l => l.MaxSpeed);
                CurrentSession.MaxLeanLeft = calculatedLaps.Max(l => l.MaxLeanLeft);
                CurrentSession.MaxLeanRight = calculatedLaps.Max(l => l.MaxLeanRight);
                CurrentSession.Laps = calculatedLaps;
                
                foreach (var lap in calculatedLaps) Laps.Add(lap);
            }
            
            if (Laps.Any())
            {
                SelectedLap = Laps.FirstOrDefault(l => l.IsBestLap) ?? Laps.First();
            }
            else
            {
                UpdateTelemetryCharts();
            }
        }

        private void AddLapSeries(List<ISeries> seriesList, LapData lap, string label, float thicknessOverride, SKColor? overrideColor)
        {
            var start = lap.StartTimeMs;
            var end = start + lap.LapTimeMs;
            var points = CurrentSession!.AllPoints
                .Where(p => p.Time >= start && p.Time <= end)
                .OrderBy(p => p.Time)
                .ToList();

            if (!points.Any()) return;

            // Mise à jour des points actuels pour le curseur (si c'est le tour sélectionné)
            if (lap == SelectedLap) _currentLapPoints = points;

            bool useDistance = (ComparisonLaps.Count > 1 || (ShowReference && ReferenceLap != null));
            double startDist = lap.StartDistance;
            bool legendAdded = false;

            if (ShowSpeed)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : SpeedThickness;
                seriesList.Add(new LineSeries<ObservablePoint>
                {
                    Values = points.Select(p => new ObservablePoint(
                        useDistance ? (double)(p.Distance - startDist) : (p.Time - start) / 1000.0, 
                        (double)p.Speed)).ToArray(),
                    Name = !legendAdded ? label : null,
                    Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(SpeedColor), thickness),
                    GeometrySize = 0,
                    Fill = null
                });
                legendAdded = true;
            }

            if (ShowAngle)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : AngleThickness;
                seriesList.Add(new LineSeries<ObservablePoint>
                {
                    Values = points.Select(p => new ObservablePoint(
                        useDistance ? (double)(p.Distance - startDist) : (p.Time - start) / 1000.0, 
                        (double)Math.Abs(p.LeanAngle))).ToArray(),
                    Name = !legendAdded ? label : null,
                    Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AngleColor), thickness),
                    GeometrySize = 0,
                    Fill = null
                });
                legendAdded = true;
            }

            if (ShowAccel)
            {
                float thickness = thicknessOverride > 0 ? thicknessOverride : AccelThickness;
                seriesList.Add(new LineSeries<ObservablePoint>
                {
                    Values = points.Select(p => new ObservablePoint(
                        useDistance ? (double)(p.Distance - startDist) : (p.Time - start) / 1000.0, 
                        (double)p.Acceleration * 50)).ToArray(),
                    Name = !legendAdded ? label : null,
                    Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AccelColor), thickness),
                    GeometrySize = 0,
                    Fill = null
                });
                legendAdded = true;
            }
        }

        public void UpdateTelemetryCharts()
        {
            if (SelectedLap == null || CurrentSession == null) return;

            bool useDistance = (ComparisonLaps.Count > 1 || (ShowReference && ReferenceLap != null));

            // Mise à jour de l'axe X
            if (useDistance)
            {
                XAxes[0].Name = "Distance (m)";
                XAxes[0].Labeler = value => $"{value:F0}";
            }
            else
            {
                XAxes[0].Name = "Temps (min:sec)";
                XAxes[0].Labeler = value => TimeSpan.FromSeconds(value).ToString(@"mm\:ss");
            }

            var seriesList = new List<ISeries>();
            LegendEntries.Clear();

            // 3. Tours de Comparaison
            var comparePool = ComparisonLaps.Where(l => l.LapTimeMs > 0 && l != SelectedLap && l != ReferenceLap).ToList();

            // Calculer les bornes globales sur TOUS les tours affichés pour une échelle cohérente
            var allVisible = new List<LapData> { SelectedLap };
            if (ShowReference && ReferenceLap != null) allVisible.Add(ReferenceLap);
            allVisible.AddRange(comparePool);

            double globalMinMs = allVisible.Min(l => l.LapTimeMs);
            double globalMaxMs = allVisible.Max(l => l.LapTimeMs);
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
                    Thickness = thickness 
                });
                
                comparisonIndex++;
            }

            // 1. Tour Sélectionné (Vert) - Calcul d'épaisseur dynamique
            string selectedTime = SelectedLap.LapTime;
            string selectedLabel = (ShowReference && SelectedLap == ReferenceLap) ? $"[REF] T{SelectedLap.Number}" : $"T{SelectedLap.Number}";
            
            double sFactor = globalRange > 0 ? (SelectedLap.LapTimeMs - globalMinMs) / globalRange : 0;
            float sThickness = (float)(CompFastThickness + (sFactor * (CompSlowThickness - CompFastThickness)));
            
            AddLapSeries(seriesList, SelectedLap, null, sThickness, null); 
            LegendEntries.Add(new LegendEntry { Label = selectedLabel, LapTime = selectedTime, Color = SpeedColor, Thickness = sThickness });

            // 2. Référence (si différente) - Calcul d'épaisseur dynamique
            if (ShowReference && ReferenceLap != null && ReferenceLap != SelectedLap)
            {
                double rFactor = globalRange > 0 ? (ReferenceLap.LapTimeMs - globalMinMs) / globalRange : 0;
                float rThickness = (float)(CompFastThickness + (rFactor * (CompSlowThickness - CompFastThickness)));
                
                AddLapSeries(seriesList, ReferenceLap, null, rThickness, SKColor.Parse(RefColor).WithAlpha(150));
                LegendEntries.Add(new LegendEntry { Label = "[REF]", LapTime = ReferenceLap.LapTime, Color = RefColor, Thickness = rThickness, IsReference = true });
            }

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
            bool hasGlobalData = false;

            foreach (var lap in allVisible)
            {
                var start = lap.StartTimeMs;
                var end = start + lap.LapTimeMs;
                var points = CurrentSession.AllPoints.Where(p => p.Time >= start && p.Time <= end).ToList();
                if (!points.Any()) continue;

                if (ShowSpeed) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)p.Speed)); 
                    maxY = Math.Max(maxY, points.Max(p => (double)p.Speed)); 
                    hasGlobalData = true; 
                }
                if (ShowAngle) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)Math.Abs(p.LeanAngle))); 
                    maxY = Math.Max(maxY, points.Max(p => (double)Math.Abs(p.LeanAngle))); 
                    hasGlobalData = true; 
                }
                if (ShowAccel) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)p.Acceleration * 50)); 
                    maxY = Math.Max(maxY, points.Max(p => (double)p.Acceleration * 50)); 
                    hasGlobalData = true; 
                }
            }

            // Adaptation dynamique du titre de l'axe Y
            var activeSeries = new List<string>();
            if (ShowSpeed) activeSeries.Add("Vitesse");
            if (ShowAngle) activeSeries.Add("Angle");
            if (ShowAccel) activeSeries.Add("G");
            YAxes[0].Name = string.Join(" / ", activeSeries);

            if (hasGlobalData && maxY > minY)
            {
                double range = maxY - minY;
                double stepY = range / 4;
                var separators = new List<double>();
                for (int i = 0; i <= 4; i++)
                {
                    double val = minY + (i * stepY);
                    separators.Add(val);
                    sectionsList.Add(new RectangularSection
                    {
                        Yi = val,
                        Yj = val,
                        Stroke = new SolidColorPaint(SKColors.White.WithAlpha(15), 0.5f)
                    });
                }
                YAxes[0].CustomSeparators = separators.ToArray();
                YAxes[0].MinLimit = minY;
                YAxes[0].MaxLimit = maxY;
            }

            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));

            // Quadrillage temporel / Distance (4 divisions verticales)
            double lapRange = 0;
            if (useDistance && _currentLapPoints.Any())
            {
                lapRange = (_currentLapPoints.Last().Distance - _currentLapPoints.First().Distance);
            }
            else
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
            UpdateCursor(0);
        }

        public void UpdateCursor(double timeOrDist)
        {
            if (!_currentLapPoints.Any()) return;

            CurrentX = timeOrDist;
            
            bool useDistance = (ComparisonLaps.Count > 1 || (ShowReference && ReferenceLap != null));
            TelemetryPoint? point = null;

            if (useDistance)
            {
                double startDist = _currentLapPoints[0].Distance;
                point = _currentLapPoints
                    .OrderBy(p => Math.Abs((p.Distance - startDist) - timeOrDist))
                    .FirstOrDefault();
            }
            else
            {
                uint targetTime = (uint)(SelectedLap.StartTimeMs + (timeOrDist * 1000.0));
                point = _currentLapPoints
                    .OrderBy(p => Math.Abs((long)p.Time - targetTime))
                    .FirstOrDefault();
            }

            if (point != null)
            {
                CurrentSpeed = point.Speed;
                CurrentAngle = Math.Abs(point.LeanAngle);
                CurrentAccel = point.Acceleration;
            }

            // Mettre à jour la position de la barre rouge dans Sections sans tout recalculer si possible
            if (Sections != null && Sections.Length > 0)
            {
                var cursorSection = Sections.Last();
                cursorSection.Xi = timeOrDist;
                cursorSection.Xj = timeOrDist;
            }
        }

        private void LoadMockupData()
        {
            var time = Enumerable.Range(0, 100).Select(i => (double)i).ToArray();
            var speeds = time.Select(t => 180 + 30 * Math.Sin(t * 0.1) + new Random(42).NextDouble() * 5).Select((v, i) => new ObservablePoint(i, v)).ToArray();
            var angles = time.Select(t => 45 * Math.Cos(t * 0.1)).Select((v, i) => new ObservablePoint(i, v)).ToArray();

            TelemetrySeries = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = speeds,
                    Name = "Vitesse (km/h)",
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 3 },
                    GeometrySize = 0
                },
                new LineSeries<ObservablePoint>
                {
                    Values = angles,
                    Name = "Angle (°)",
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 3 },
                    GeometrySize = 0
                }
            };
        }
        private void UpdateTrajectoryUI(TrackMap map)
        {
            if (map.Trajectory.Count == 0) return;

            double minLat = map.Trajectory.Min(p => p.Latitude);
            double maxLat = map.Trajectory.Max(p => p.Latitude);
            double minLon = map.Trajectory.Min(p => p.Longitude);
            double maxLon = map.Trajectory.Max(p => p.Longitude);

            double latRange = maxLat - minLat;
            double lonRange = maxLon - minLon;

            // Protection division par zéro
            if (latRange == 0) latRange = 0.0001;
            if (lonRange == 0) lonRange = 0.0001;

            double canvasWidth = 300; // Taille virtuelle pour le binding
            double canvasHeight = 300;

            // On garde l'aspect ratio
            double ratio = Math.Cos(minLat * Math.PI / 180.0);
            double scaleLat = canvasHeight / latRange;
            double scaleLon = canvasWidth / (lonRange * ratio);
            double scale = Math.Min(scaleLat, scaleLon) * 0.8; // 80% du canvas

            var points = new System.Windows.Media.PointCollection();
            foreach (var p in map.Trajectory)
            {
                // Inversion Y car en UI 0 est en haut
                double x = (p.Longitude - minLon) * ratio * scale + (canvasWidth / 10);
                double y = canvasHeight - ((p.Latitude - minLat) * scale + (canvasHeight / 10));
                points.Add(new System.Windows.Point(x, y));
            }

            TrajectoryPoints = points;
        }

        private string? FindMatchingMap(string sessionFilePath)
        {
            string sessionDir = System.IO.Path.GetDirectoryName(sessionFilePath) ?? "";
            string sessionFolderName = System.IO.Path.GetFileName(sessionDir); // e.g. LEDENON-2026-04-12
            
            // Extract circuit name and normalize
            string circuitSearch = NormalizeString(sessionFolderName.Split('-')[0]);

            string mapsRoot = @"C:\dev\3DMS-CED\Map\Circuits";
            if (!System.IO.Directory.Exists(mapsRoot)) return null;

            foreach (var countryDir in System.IO.Directory.GetDirectories(mapsRoot))
            {
                foreach (var mapFile in System.IO.Directory.GetFiles(countryDir, "*.map"))
                {
                    string mapName = NormalizeString(System.IO.Path.GetFileNameWithoutExtension(mapFile));
                    // Check for overlap
                    if (mapName.Contains(circuitSearch) || circuitSearch.Contains(mapName))
                    {
                        return mapFile;
                    }
                }
            }
            return null;
        }

        private string NormalizeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            // Remove accents and set to lower
            return new string(text.Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Normalize(System.Text.NormalizationForm.FormC)
                .ToLower();
        }
        private double ParseTimeToMs(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr) || timeStr == "-") return 0;
            try
            {
                var parts = timeStr.Split(':');
                if (parts.Length < 2) return 0;
                
                double minutes = double.Parse(parts[0]);
                double seconds = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                return (minutes * 60 + seconds) * 1000.0;
            }
            catch { return 0; }
        }

        private string FormatTimeFromMs(double ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            return string.Format("{0:D2}:{1:D2}.{2:D2}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }
        private DateTime ParseDateFromFileName(string fileName)
        {
            try
            {
                // Format attendu : CIRCUIT-YYYY-MM-DD-HHhMM ou CIRCUIT_YYYY-MM-DD_HHhMM
                var match = System.Text.RegularExpressions.Regex.Match(fileName, @"(\d{4}-\d{2}-\d{2})[_-](\d{2})[hH](\d{2})");
                if (match.Success)
                {
                    var dateParts = match.Groups[1].Value.Split('-');
                    int year = int.Parse(dateParts[0]);
                    int month = int.Parse(dateParts[1]);
                    int day = int.Parse(dateParts[2]);
                    int hour = int.Parse(match.Groups[2].Value);
                    int minute = int.Parse(match.Groups[3].Value);

                    return new DateTime(year, month, day, hour, minute, 0);
                }

                // Fallback 1 : juste la date CIRCUIT-YYYY-MM-DD
                var dateMatch = System.Text.RegularExpressions.Regex.Match(fileName, @"(\d{4}-\d{2}-\d{2})");
                if (dateMatch.Success)
                {
                    var dateParts = dateMatch.Groups[1].Value.Split('-');
                    return new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]));
                }
            }
            catch { /* Fallback sur DateTime.Now */ }

            return DateTime.Now;
        }
    }
}
