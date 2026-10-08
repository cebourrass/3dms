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
        // Cache pour la détection rapide par dossier
        private readonly Dictionary<string, CircuitMetadata> _directoryCircuitCache = new();

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
                    UpdateTrajectoryUI(CurrentMap);
                }
            }
        }

        private void LoadAvailableCircuits()
        {
            _availableCircuits.Clear();
            string mapsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Map", "Circuits");
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

            if (bestMatch != null && bestMatch.Name.Contains("Alès"))
            {
                // Priorité par défaut au sens horaire pour Alès
                var horaire = _availableCircuits.FirstOrDefault(c => c.Name == "Alès (Sens horaire)");
                if (horaire != null) return horaire;
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

        private string? FindMatchingMap(string sessionFilePath)
        {
            string sessionDir = System.IO.Path.GetDirectoryName(sessionFilePath) ?? "";
            string sessionFolderName = System.IO.Path.GetFileName(sessionDir); // e.g. LEDENON-2026-04-12
            
            // Extract circuit name and normalize
            string circuitSearch = NormalizeString(sessionFolderName.Split('-')[0]);

            string mapsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Map", "Circuits");
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
    }
}
