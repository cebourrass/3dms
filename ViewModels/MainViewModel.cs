using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Analyzer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
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
            // Génération de données factices pour le mockup
            var time = Enumerable.Range(0, 100).Select(i => (double)i).ToArray();
            var speeds = time.Select(t => 180 + 30 * Math.Sin(t * 0.1) + new Random(42).NextDouble() * 5).ToArray();
            var angles = time.Select(t => 45 * Math.Cos(t * 0.1)).ToArray();

            _speedSeries = new ISeries[]
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

            _angleSeries = new ISeries[]
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
