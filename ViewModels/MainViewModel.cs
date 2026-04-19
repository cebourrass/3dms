using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Analyzer.Services;
using Analyzer.Models;
using System.Collections.Generic;

namespace Analyzer.ViewModels
{
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

        private ISeries[] _speedSeries;
        public ISeries[] SpeedSeries
        {
            get => _speedSeries;
            set => SetProperty(ref _speedSeries, value);
        }

        private ISeries[] _angleSeries;
        public ISeries[] AngleSeries
        {
            get => _angleSeries;
            set => SetProperty(ref _angleSeries, value);
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

        private ObservableCollection<ExplorerItem> _explorerItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get => _explorerItems;
            set => SetProperty(ref _explorerItems, value);
        }

        private ExplorerItem _selectedExplorerItem;
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

        public Axis[] XAxes { get; set; } = 
        {
            new Axis
            {
                Name = "Temps (s)",
                Labeler = value => value.ToString("F1")
            }
        };

        public MainViewModel()
        {
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

        private SessionItem FindFirstSession(IEnumerable<ExplorerItem> items)
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
        public void LoadSession(string filePath)
        {
            var points = _readerService.ReadFile(filePath);
            if (points == null || !points.Any()) return;

            var session = new SessionData
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                AllPoints = points
            };

            string? mapFile = FindMatchingMap(filePath);
            if (mapFile != null)
            {
                session.MapFilePath = mapFile;
                session.CircuitMap = _mapReaderService.ReadMap(mapFile);
                
                // Le nombre de secteurs est le nombre de marqueurs Time + 1 (pour rejoindre l'arrivée)
                int markerTimes = session.CircuitMap.Markers.Keys.Count(k => k.StartsWith("Time", StringComparison.OrdinalIgnoreCase));
                session.PartialCount = markerTimes > 0 ? markerTimes + 1 : 0;
                
                CurrentMap = session.CircuitMap;
                CircuitName = CurrentMap.Name;
                UpdateTrajectoryUI(CurrentMap);
            }
            else
            {
                CircuitName = "Circuit inconnu";
                TrajectoryPoints = new System.Windows.Media.PointCollection();
            }

            SessionTitle = session.Title;
            Laps.Clear();
            
            // Calcul des tours réels
            if (session.CircuitMap != null)
            {
                var calculatedLaps = _lapService.CalculateLaps(points, session.CircuitMap);
                if (calculatedLaps.Any())
                {
                    // Seuls les tours complets comptent pour les records
                    var completeLaps = calculatedLaps.Where(l => l.Type == "Complet").ToList();
                    
                    if (completeLaps.Any())
                    {
                        var best = completeLaps.OrderBy(l => l.LapTimeMs).First();
                        best.IsBestLap = true;
                        session.BestLapTime = best.LapTime;
                        BestLapTime = best.LapTime;

                        // Calcul du Temps Idéal (somme des meilleurs secteurs de tous les tours complets)
                        double idealMs = 0;
                        int numSectors = session.PartialCount;
                        if (numSectors > 0)
                        {
                            for (int s = 0; s < numSectors; s++)
                            {
                                // On cherche le min pour ce secteur s parmi les tours complets
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

                    var maxSpd = calculatedLaps.Max(l => l.MaxSpeed);
                    foreach (var l in calculatedLaps.Where(lap => lap.MaxSpeed == maxSpd)) l.IsMaxSpeed = true;
                    session.MaxSpeed = maxSpd;

                    var maxAng = calculatedLaps.Max(l => Math.Max(l.MaxLeanLeft, l.MaxLeanRight));
                    foreach (var l in calculatedLaps.Where(lap => lap.MaxLeanLeft == maxAng || lap.MaxLeanRight == maxAng)) l.IsMaxAngle = true;
                    session.MaxLeanLeft = calculatedLaps.Max(l => l.MaxLeanLeft);
                    session.MaxLeanRight = calculatedLaps.Max(l => l.MaxLeanRight);
                    
                    session.Laps = calculatedLaps;
                    foreach (var lap in calculatedLaps) Laps.Add(lap);
                }
            }
            
            CurrentSession = session;

            // On ne prend qu'un point sur 2 pour l'affichage (10Hz -> 5Hz)
            var displayPoints = points.Where((p, i) => i % 2 == 0).ToList();

            var speedValues = displayPoints.Select(p => (double)p.Speed).ToArray();
            var angleValues = displayPoints.Select(p => (double)p.LeanAngle).ToArray();

            SpeedSeries = new ISeries[] 
            { 
                new LineSeries<double> 
                { 
                    Values = speedValues, 
                    Name = "Vitesse", 
                    Stroke = new SolidColorPaint(SKColors.Cyan, 2),
                    GeometrySize = 0,
                    Fill = null
                } 
            };
            
            AngleSeries = new ISeries[] 
            { 
                new LineSeries<double> 
                { 
                    Values = angleValues, 
                    Name = "Angle", 
                    Stroke = new SolidColorPaint(SKColors.Yellow, 2),
                    GeometrySize = 0,
                    Fill = null
                } 
            };
        }

        private void LoadMockupData()
        {
            var time = Enumerable.Range(0, 100).Select(i => (double)i).ToArray();
            var speeds = time.Select(t => 180 + 30 * Math.Sin(t * 0.1) + new Random(42).NextDouble() * 5).ToArray();
            var angles = time.Select(t => 45 * Math.Cos(t * 0.1)).ToArray();

            SpeedSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = speeds,
                    Name = "Vitesse (km/h)",
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 3 },
                    GeometrySize = 0
                }
            };

            AngleSeries = new ISeries[]
            {
                new LineSeries<double>
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
    }
}
