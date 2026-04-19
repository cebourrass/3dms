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
            // Initialisation par défaut avec un fichier si présent, sinon données mockup
            string defaultPath = @"C:\dev\3DMS-CED\3DMS Evo (38.39.8F.DC.D1.31)\LEDENON-2026-04-12\2026-04-12 a 10h29.ra1";
            if (System.IO.File.Exists(defaultPath))
            {
                LoadSession(defaultPath);
            }
            else
            {
                LoadMockupData();
            }
        }

        [RelayCommand]
        public void LoadSession(string filePath)
        {
            var points = _readerService.ReadFile(filePath);
            if (points == null || !points.Any()) return;

            // On ne prend qu'un point sur 5 pour l'affichage si c'est trop dense (facultatif)
            var displayPoints = points.Where((p, i) => i % 2 == 0).ToList();

            var times = displayPoints.Select(p => (double)p.Time / 1000.0).ToArray();
            var speeds = displayPoints.Select(p => (double)p.Speed).ToArray();
            var angles = displayPoints.Select(p => (double)p.LeanAngle).ToArray();

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
    }
}
