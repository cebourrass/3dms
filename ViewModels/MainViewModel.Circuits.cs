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

        private readonly CircuitCatalog _circuitCatalog = new CircuitCatalog();

        private void LoadAvailableCircuits()
        {
            _circuitCatalog.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Map", "Circuits"));
            _availableCircuits.Clear();
            foreach (var c in _circuitCatalog.Circuits) _availableCircuits.Add(c);
        }

        private CircuitMetadata? DetectCircuit(List<TelemetryPoint> points) => _circuitCatalog.Detect(points);

        private void ApplyCircuit(string mapFilePath)
        {
            if (CurrentSession == null) return;

            // Mettre à jour le cache pour ce dossier pour les prochains fichiers
            string directory = System.IO.Path.GetDirectoryName(CurrentSession.FilePath) ?? "";
            var circuit = AvailableCircuits.FirstOrDefault(c => c.FilePath == mapFilePath);
            if (circuit != null) _directoryCircuitCache[directory] = circuit;
            
            CurrentSession.MapFilePath = mapFilePath;
            CurrentSession.CircuitMap = _mapReaderService.ReadMap(mapFilePath);
            
            CurrentSession.PartialCount = LapStatistics.SectorCount(CurrentSession.CircuitMap);
            
            CurrentMap = CurrentSession.CircuitMap;
            CircuitName = CurrentMap.Name;
            UpdateTrajectoryUI(CurrentMap);

            RecalculateLaps();
        }

        private string? FindMatchingMap(string sessionFilePath) => _circuitCatalog.FindByFolderName(sessionFilePath)?.FilePath;
    }
}
