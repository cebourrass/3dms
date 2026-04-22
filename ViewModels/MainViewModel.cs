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
    public class CursorLapValue : ObservableObject
    {
        private string _lapName = string.Empty;
        public string LapName { get => _lapName; set => SetProperty(ref _lapName, value); }

        private string _color = "#FFFFFF";
        public string Color { get => _color; set => SetProperty(ref _color, value); }

        private double _speed;
        public double Speed { get => _speed; set => SetProperty(ref _speed, value); }

        private double _angle;
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }

        private double _accel;
        public double Accel { get => _accel; set => SetProperty(ref _accel, value); }
    }

    public class LegendEntry : ObservableObject
    {
        public string Label { get; set; } = string.Empty;
        public string LapTime { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF";
        public double Thickness { get; set; } = 1.5;
        public bool IsReference { get; set; }
        public double SortTimeMs { get; set; }
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
        private readonly SettingsService _settingsService = new SettingsService();
        private UserSettings _settings;

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
        
        private double _currentSpeed;
        public double CurrentSpeed { get => _currentSpeed; set => SetProperty(ref _currentSpeed, value); }
        
        private double _currentAngle;
        public double CurrentAngle { get => _currentAngle; set => SetProperty(ref _currentAngle, value); }
        
        private float _currentAccel;
        public float CurrentAccel { get => _currentAccel; set => SetProperty(ref _currentAccel, value); }

        private string _currentAngleColor = "#fbbf24";
        public string CurrentAngleColor { get => _currentAngleColor; set => SetProperty(ref _currentAngleColor, value); }

        private string _currentAccelColor = "#8b5cf6";
        public string CurrentAccelColor { get => _currentAccelColor; set => SetProperty(ref _currentAccelColor, value); }

        // Cache pour la détection rapide par dossier
        private readonly Dictionary<string, CircuitMetadata> _directoryCircuitCache = new();

        // Cache indépendant pour la référence globale (évite les pbs lors du switch de RA1)
        private List<TelemetryPoint>? _cachedReferencePoints = null;
        private string _cachedReferenceTime = "--:--.--";
        
        public ObservableCollection<LegendEntry> LegendEntries { get; } = new();

        public SolidColorPaint LegendTextPaint { get; } = new SolidColorPaint(new SKColor(200, 200, 200));

        private List<TelemetryPoint> _currentLapPoints = new();
        private List<TelemetryPoint>? _interpolatedPoints;
        public ObservableCollection<RegularityItem> RegularityStats { get; } = new();

        private bool _isRegularityVisible;
        public bool IsRegularityVisible { get => _isRegularityVisible; set => SetProperty(ref _isRegularityVisible, value); }

        private double _regularityThresholdExcellent = 0.10;
        public double RegularityThresholdExcellent 
        { 
            get => _regularityThresholdExcellent; 
            set { if (SetProperty(ref _regularityThresholdExcellent, value)) UpdateRegularityStats(); } 
        }

        private double _regularityThresholdMedium = 0.30;
        public double RegularityThresholdMedium 
        { 
            get => _regularityThresholdMedium; 
            set { if (SetProperty(ref _regularityThresholdMedium, value)) UpdateRegularityStats(); } 
        }

        private string _xAxisValueLabel = "TEMPS";
        public string XAxisValueLabel => _xAxisValueLabel;
        public string XAxisUnit => _xAxisValueLabel == "DISTANCE" ? "m" : "s";

        public ObservableCollection<CursorLapValue> CursorLaps { get; } = new();

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
        public bool IsChartsVisible { get => _isChartsVisible; set => SetProperty(ref _isChartsVisible, value); }

        private bool _isExplorerVisible = true;
        public bool IsExplorerVisible { get => _isExplorerVisible; set => SetProperty(ref _isExplorerVisible, value); }

        public MainViewModel()
        {
            _settings = _settingsService.LoadSettings();

            // Appliquer les paramètres chargés
            _showSpeed = _settings.ShowSpeed;
            _showAngleLeft = _settings.ShowAngleLeft;
            _showAngleRight = _settings.ShowAngleRight;
            _showAccel = _settings.ShowAccel;
            _showDecel = _settings.ShowDecel;
            _showReference = _settings.ShowReference;

            _speedColor = _settings.SpeedColor ?? "#10b981";
            _speedThickness = _settings.SpeedThickness;
            _angleColor = _settings.AngleColor ?? "#fbbf24";
            _angleThickness = _settings.AngleThickness;
            _angleRightColor = _settings.AngleRightColor ?? "#f59e0b";
            _angleRightThickness = _settings.AngleRightThickness;
            _accelColor = _settings.AccelColor ?? "#8b5cf6";
            _accelThickness = _settings.AccelThickness;
            _decelColor = _settings.DecelColor ?? "#ef4444";
            _decelThickness = _settings.DecelThickness;
            _refColor = _settings.RefColor;
            _refThickness = _settings.RefThickness;
            _compFastThickness = _settings.CompFastThickness;
            _compSlowThickness = _settings.CompSlowThickness;

            _isLapsVisible = _settings.IsLapsVisible;
            _isSessionInfoVisible = _settings.IsSessionInfoVisible;
            _isMapVisible = _settings.IsMapVisible;
            _isChartsVisible = _settings.IsChartsVisible;
            _isExplorerVisible = _settings.IsExplorerVisible;

            LoadAvailableCircuits();
            LoadExplorer();

            if (!string.IsNullOrEmpty(_settings.LastFilePath) && File.Exists(_settings.LastFilePath))
            {
                // Recharge asynchrone pour laisser l'UI s'initialiser
                System.Threading.Tasks.Task.Run(async () => {
                    await System.Threading.Tasks.Task.Delay(500);
                    App.Current.Dispatcher.Invoke(() => LoadSession(_settings.LastFilePath));
                });
            }
            else
            {
                LoadDummyLaps();
                CircuitName = "Aucun circuit";
            }
        }

        public void SaveSettings(double width, double height, string windowState)
        {
            _settings.WindowWidth = width;
            _settings.WindowHeight = height;
            _settings.WindowState = windowState;

            _settings.IsLapsVisible = IsLapsVisible;
            _settings.IsSessionInfoVisible = IsSessionInfoVisible;
            _settings.IsMapVisible = IsMapVisible;
            _settings.IsChartsVisible = IsChartsVisible;
            _settings.IsExplorerVisible = IsExplorerVisible;

            _settings.ShowSpeed = ShowSpeed;
            _settings.ShowAngleLeft = ShowAngleLeft;
            _settings.ShowAngleRight = ShowAngleRight;
            _settings.ShowAccel = ShowAccel;
            _settings.ShowDecel = ShowDecel;
            _settings.ShowReference = ShowReference;

            _settings.SpeedColor = SpeedColor;
            _settings.SpeedThickness = SpeedThickness;
            _settings.AngleColor = AngleColor;
            _settings.AngleThickness = AngleThickness;
            _settings.AngleRightColor = AngleRightColor;
            _settings.AngleRightThickness = AngleRightThickness;
            _settings.AccelColor = AccelColor;
            _settings.AccelThickness = AccelThickness;
            _settings.DecelColor = DecelColor;
            _settings.DecelThickness = DecelThickness;
            _settings.RefColor = RefColor;
            _settings.RefThickness = RefThickness;

            _settings.CompFastThickness = CompFastThickness;
            _settings.CompSlowThickness = CompSlowThickness;

            _settings.LastFilePath = CurrentSession?.FilePath;

            _settingsService.SaveSettings(_settings);
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
            if (SelectedLap == null || CurrentSession == null) return;
            
            // On désactive l'ancien indicateur visuel
            foreach (var l in Laps) l.IsReference = false;
            
            // On stocke l'objet
            ReferenceLap = SelectedLap;
            ReferenceLap.IsReference = true;

            // CAPTURE GLOBALE : On clone les données pour qu'elles survivent au changement de session
            _cachedReferenceTime = ReferenceLap.LapTime;
            
            uint startT = (uint)ReferenceLap.StartTimeMs;
            float startD = (float)ReferenceLap.StartDistance;
            
            _cachedReferencePoints = CurrentSession.AllPoints
                .Where(p => p.Time >= startT && p.Time <= (startT + ReferenceLap.LapTimeMs + 500))
                .Select(p => new TelemetryPoint {
                    Time = p.Time - startT,
                    Distance = p.Distance - startD,
                    Speed = p.Speed,
                    LeanAngle = p.LeanAngle,
                    Acceleration = p.Acceleration
                })
                .ToList();

            UpdateTelemetryCharts();
        }

        [RelayCommand]
        public void ClearReference()
        {
            if (ReferenceLap != null) ReferenceLap.IsReference = false;
            ReferenceLap = null;
            _cachedReferencePoints = null;
            _cachedReferenceTime = "--:--.--";
            UpdateTelemetryCharts();
        }

        [RelayCommand]
        public void LoadSession(string filePath)
        {
            var points = _readerService.ReadFile(filePath);
            if (points == null || !points.Any()) return;

            // Clear previous session state completely
            Laps.Clear();
            // On ne vide plus ReferenceLap pour permettre la comparaison globale
            ComparisonLaps.Clear();
            _currentLapPoints.Clear();
            BestLapTime = "--:--.--";
            IdealLapTime = "--:--.--";
            CircuitName = "Chargement...";
            TrajectoryPoints = new System.Windows.Media.PointCollection();

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

            // Circuit Detection avec Cache par dossier
            string directory = System.IO.Path.GetDirectoryName(filePath) ?? "";
            CircuitMetadata? detected = null;

            if (_directoryCircuitCache.TryGetValue(directory, out var cached))
            {
                detected = cached;
            }
            else
            {
                detected = DetectCircuit(points);
                if (detected != null) _directoryCircuitCache[directory] = detected;
            }

            if (detected != null)
            {
                _selectedCircuit = detected; 
                OnPropertyChanged(nameof(SelectedCircuit));
                ApplyCircuit(detected.FilePath);
            }
            else
            {
                // Fallback to name detection
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

            // Mettre à jour le cache pour ce dossier pour les prochains fichiers
            string directory = System.IO.Path.GetDirectoryName(CurrentSession.FilePath) ?? "";
            var circuit = AvailableCircuits.FirstOrDefault(c => c.FilePath == mapFilePath);
            if (circuit != null) _directoryCircuitCache[directory] = circuit;
            
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
                var end = start + lap.LapTimeMs;
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
                lap.TelemetryPoints = points;
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
                    LineSmoothness = smoothness
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
                        LineSmoothness = smoothness
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
                        LineSmoothness = smoothness
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
                    // Courbe Accélération (Points positifs)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.Acceleration >= 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)p.Acceleration * 50)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = !legendAdded ? (label != null ? $"{label} (Acc)" : "Accel") : null,
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(AccelColor), thickness),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness
                    });
                }

                if (ShowDecel)
                {
                    // Courbe Décélération (Points négatifs mis en positif)
                    seriesList.Add(new LineSeries<ObservablePoint?>
                    {
                        Values = processedPoints.Select(p => p.Acceleration < 0 
                            ? new ObservablePoint(useDistance ? (double)(p.Distance - startDist) : (p.Time - lapStart) / 1000.0, (double)-p.Acceleration * 50)
                            : (ObservablePoint?)null
                        ).ToArray(),
                        Name = label != null ? $"{label} (Frein)" : "Frein",
                        Stroke = new SolidColorPaint(overrideColor ?? SKColor.Parse(DecelColor), thicknessDecel),
                        GeometrySize = 0,
                        Fill = null,
                        LineSmoothness = smoothness
                    });
                }
                legendAdded = true;
            }
        }

        private List<TelemetryPoint> InterpolateAndSmooth(List<TelemetryPoint> rawPoints)
        {
            if (rawPoints.Count < 2) return rawPoints;

            var smoothed = new List<TelemetryPoint>();
            int window = 3;
            for (int i = 0; i < rawPoints.Count; i++)
            {
                int start = Math.Max(0, i - window / 2);
                int end = Math.Min(rawPoints.Count - 1, i + window / 2);
                int count = end - start + 1;

                var avg = new TelemetryPoint
                {
                    Time = rawPoints[i].Time,
                    Distance = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.Distance),
                    Speed = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.Speed),
                    LeanAngle = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.LeanAngle),
                    Acceleration = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.Acceleration),
                    Latitude = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.Latitude),
                    Longitude = (float)rawPoints.Skip(start).Take(count).Average(p => (double)p.Longitude)
                };
                smoothed.Add(avg);
            }

            var interpolated = new List<TelemetryPoint>();
            double startTime = rawPoints.First().Time;
            double endTime = rawPoints.Last().Time;
            double step = 20.0;

            for (double t = startTime; t <= endTime; t += step)
            {
                var p1 = smoothed.LastOrDefault(p => p.Time <= t);
                var p2 = smoothed.FirstOrDefault(p => p.Time > t);

                if (p1 != null && p2 != null)
                {
                    float ratio = (float)((t - p1.Time) / (p2.Time - p1.Time));
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
                else if (p1 != null)
                {
                    interpolated.Add(p1);
                }
            }

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
            bool hasGlobalData = false;

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
                    hasGlobalData = true; 
                }
                if (IsAnyAngleVisible) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)Math.Abs(p.LeanAngle))); 
                    maxY = Math.Max(maxY, points.Max(p => (double)Math.Abs(p.LeanAngle))); 
                    hasGlobalData = true; 
                }
                if (IsAnyAccelVisible) 
                { 
                    minY = Math.Min(minY, points.Min(p => (double)Math.Abs(p.Acceleration) * 50)); 
                    maxY = Math.Max(maxY, points.Max(p => (double)Math.Abs(p.Acceleration) * 50)); 
                    hasGlobalData = true; 
                }
            }

            // Adaptation dynamique du titre de l'axe Y
            var activeSeries = new List<string>();
            if (ShowSpeed) activeSeries.Add("Vitesse");
            if (IsAnyAngleVisible) activeSeries.Add("Angle");
            if (IsAnyAccelVisible) activeSeries.Add("G");
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
            UpdateCursor(CurrentX, true);

            // Calculer les statistiques de régularité
            UpdateRegularityStats();
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
                    CurrentSpeed = point.Speed;
                    CurrentAngle = Math.Abs(point.LeanAngle);
                    CurrentAngleColor = point.LeanAngle <= 0 ? AngleColor : AngleRightColor;
                    CurrentAccel = Math.Abs(point.Acceleration);
                    CurrentAccelColor = point.Acceleration >= 0 ? AccelColor : DecelColor;
                }
            }

            // 2. Mettre à jour CursorLaps pour TOUS les tours comparés (Réutilisation des objets pour la fluidité)
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

            // 3. Mettre à jour la position de la barre rouge
            if (Sections != null && Sections.Length > 0)
            {
                var cursorSection = Sections.Last();
                cursorSection.Xi = timeOrDist;
                cursorSection.Xj = timeOrDist;
            }
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

        private void UpdateRegularityStats()
        {
            RegularityStats.Clear();
            var laps = ComparisonLaps.Where(l => l.LapTimeMs > 0).ToList();
            
            if (laps.Count < 2)
            {
                IsRegularityVisible = false;
                return;
            }

            IsRegularityVisible = true;
            AddRegularityItem("Total", laps.Select(l => (double)l.LapTimeMs / 1000.0).ToList());

            int numSectors = CurrentSession?.PartialCount ?? 0;
            for (int i = 0; i < numSectors; i++)
            {
                int sectorIndex = i;
                var sectorTimes = laps
                    .Select(l => ParseTimeToMs(l.Partials[sectorIndex]) / 1000.0)
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

    public class RegularityItem
    {
        public string Label { get; set; } = "";
        public double StdDev { get; set; }
        public string Color { get; set; } = "#FFFFFF";
    }
}
