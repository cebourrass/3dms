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
        private readonly Ra1ReaderService _readerService = new Ra1ReaderService();
        private readonly MapReaderService _mapReaderService = new MapReaderService();
        private readonly LapService _lapService = new LapService();
        private readonly SettingsService _settingsService = new SettingsService();
        private readonly CornerService _cornerService = new CornerService();
        private UserSettings _settings;

        private LapData? _referenceLap;
        public LapData? ReferenceLap
        {
            get => _referenceLap;
            set { if (SetProperty(ref _referenceLap, value)) UpdateTelemetryCharts(); }
        }

        private bool _showReference = true;
        public bool ShowReference { get => _showReference; set { if (SetProperty(ref _showReference, value)) UpdateTelemetryCharts(); } }

        public List<LapData> ComparisonLaps { get; set; } = new();

        private string _currentLanguage = "French";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (SetProperty(ref _currentLanguage, value))
                {
                    SwitchLanguage(value);
                    if (_settings != null) _settings.Language = value;
                }
            }
        }

        public List<string> AvailableLanguages { get; } = new() { "French", "English" };

        private void SwitchLanguage(string lang)
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            // Mettre à jour la culture du thread
            var culture = new System.Globalization.CultureInfo(lang == "French" ? "fr-FR" : "en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // Retirer l'ancien dictionnaire de langue
            var oldDict = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("French.xaml") || d.Source.OriginalString.Contains("English.xaml")));
            
            if (oldDict != null) app.Resources.MergedDictionaries.Remove(oldDict);

            // Ajouter le nouveau
            var newDict = new System.Windows.ResourceDictionary();
            newDict.Source = new Uri($"Languages/{lang}.xaml", UriKind.Relative);
            app.Resources.MergedDictionaries.Add(newDict);

            // Forcer la mise à jour des axes
            UpdateTelemetryCharts();
        }

        // Cache indépendant pour la référence globale (évite les pbs lors du switch de RA1)
        private List<TelemetryPoint>? _cachedReferencePoints = null;
        private string _cachedReferenceTime = "--:--.--";

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

        private ObservableCollection<LapData> _laps = new ObservableCollection<LapData>();
        public ObservableCollection<LapData> Laps
        {
            get => _laps;
            set => SetProperty(ref _laps, value);
        }

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

        private bool _showCorruptionWarning = false;
        public bool ShowCorruptionWarning 
        { 
            get => _showCorruptionWarning; 
            set 
            { 
                if (SetProperty(ref _showCorruptionWarning, value))
                    OnPropertyChanged(nameof(IsCorruptionBadgeVisible));
            } 
        }

        public bool IsCorruptionBadgeVisible => ShowCorruptionWarning && (CurrentSession?.HasCorruptedData ?? false);

        public MainViewModel()
        {
            _settings = _settingsService.LoadSettings();
            CurrentLanguage = _settings.Language ?? "French";

            // Appliquer les paramètres chargés
            _showSpeed = _settings.ShowSpeed;
            _showAngleLeft = _settings.ShowAngleLeft;
            _showAngleRight = _settings.ShowAngleRight;
            _showAccel = _settings.ShowAccel;
            _showDecel = _settings.ShowDecel;
            _showReference = _settings.ShowReference;
            _showAccelMapGradient = _settings.ShowAccelMapGradient;
            _showCorruptionWarning = _settings.ShowCorruptionWarning;

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
            _mapCompFastThickness = _settings.MapCompFastThickness > 0 ? _settings.MapCompFastThickness : 1.0f;
            _mapCompSlowThickness = _settings.MapCompSlowThickness > 0 ? _settings.MapCompSlowThickness : 0.3f;

            _speedSmoothing = _settings.SpeedSmoothing;
            _angleSmoothing = _settings.AngleSmoothing;
            _accelSmoothing = _settings.AccelSmoothing;
            _gpsSmoothing = _settings.GpsSmoothing;
            _interpolationStepMs = _settings.InterpolationStepMs > 0 ? _settings.InterpolationStepMs : 20.0;
            _mapTrajectoryThickness = _settings.MapTrajectoryThickness > 0 ? _settings.MapTrajectoryThickness : 1.2;
            _mapCursorSize = _settings.MapCursorSize > 0 ? _settings.MapCursorSize : 6.0;

            _accelGradientRange = _settings.AccelGradientRange > 0 ? _settings.AccelGradientRange : 1.2;
            _autoAccelGradientScaling = _settings.AutoAccelGradientScaling;
            _autoCenterMap = _settings.AutoCenterMap;

            _showDeltaTime = false; // Par défaut éteint
            
            _regularityThresholdExcellent = _settings.RegularityThresholdExcellent;
            _regularityThresholdMedium = _settings.RegularityThresholdMedium;
            _cornerEntryAngle = _settings.CornerEntryAngle > 0 ? _settings.CornerEntryAngle : 15;
            _cornerExitAngle = _settings.CornerExitAngle > 0 ? _settings.CornerExitAngle : 8;
            _cornerMinLength = _settings.CornerMinLength > 0 ? _settings.CornerMinLength : 30;
            _showCornerMarkers = _settings.ShowCornerMarkers;
            SelectedPilotProfile = PilotProfiles.FirstOrDefault(p => p.Name == _settings.SelectedPilotProfileName) ?? PilotProfiles[1];

            _isLapsVisible = _settings.IsLapsVisible;
            _isSessionInfoVisible = _settings.IsSessionInfoVisible;
            _isMapVisible = _settings.IsMapVisible;
            _isChartsVisible = _settings.IsChartsVisible;
            _isExplorerVisible = _settings.IsExplorerVisible;

            if (!string.IsNullOrWhiteSpace(_settings.DataFolderPath))
                _dataFolderPath = _settings.DataFolderPath;

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
            _settings.ShowAccelMapGradient = ShowAccelMapGradient;
            _settings.AccelGradientRange = AccelGradientRange;
            _settings.AutoAccelGradientScaling = AutoAccelGradientScaling;
            _settings.AutoCenterMap = AutoCenterMap;
            _settings.ShowCorruptionWarning = ShowCorruptionWarning;

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
            _settings.MapCompFastThickness = MapCompFastThickness;
            _settings.MapCompSlowThickness = MapCompSlowThickness;

            _settings.SpeedSmoothing = SpeedSmoothing;
            _settings.AngleSmoothing = AngleSmoothing;
            _settings.AccelSmoothing = AccelSmoothing;
            _settings.GpsSmoothing = GpsSmoothing;
            _settings.InterpolationStepMs = InterpolationStepMs;
            _settings.MapTrajectoryThickness = MapTrajectoryThickness;
            _settings.MapCursorSize = MapCursorSize;

            _settings.LastFilePath = CurrentSession?.FilePath;
            _settings.SelectedPilotProfileName = SelectedPilotProfile?.Name;
            _settings.RegularityThresholdExcellent = RegularityThresholdExcellent;
            _settings.RegularityThresholdMedium = RegularityThresholdMedium;
            _settings.CornerEntryAngle = CornerEntryAngle;
            _settings.CornerExitAngle = CornerExitAngle;
            _settings.CornerMinLength = CornerMinLength;
            _settings.ShowCornerMarkers = ShowCornerMarkers;

            _settingsService.SaveSettings(_settings);
        }

        private void LoadDummyLaps()
        {
            Laps.Clear();
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
                    OnPropertyChanged(nameof(IsCorruptionBadgeVisible));
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
            ShowReference = true;
        }

        [RelayCommand]
        public void ClearReference()
        {
            if (ReferenceLap != null) ReferenceLap.IsReference = false;
            ReferenceLap = null;
            _cachedReferencePoints = null;
            _cachedReferenceTime = "--:--.--";
            UpdateTelemetryCharts();
            ShowReference = false;
        }

        [RelayCommand]
        public void LoadSession(string filePath)
        {
            var points = _readerService.ReadFile(filePath);
            if (points == null || !points.Any()) return;

            // Clear previous session state completely
            Laps.Clear();
            ReferenceLap = null;
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
                Date = ParseDateFromFileName(fileName),
                HasCorruptedData = _readerService.LastCorruptedPointsCount > 0,
                CorruptedPointsCount = _readerService.LastCorruptedPointsCount
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

        private System.Windows.Threading.DispatcherTimer? _cursorDebounceTimer;

        private void LoadMockupData()
        {
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
