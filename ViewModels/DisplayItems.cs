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

        private string _lapTime = string.Empty;
        public string LapTime { get => _lapTime; set => SetProperty(ref _lapTime, value); }
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

    public class PilotProfile
    {
        public string Name { get; set; } = string.Empty;
        public double Excellent { get; set; }
        public double Medium { get; set; }
        public override string ToString() => Name;
    }

    public class LapTrajectory : ObservableObject
    {
        public string Color { get; set; } = "#FFFFFF";
        public System.Windows.Media.PointCollection Points { get; set; } = new();
        public double Thickness { get; set; } = 1.5;
        public float Opacity { get; set; } = 1.0f;
    }

    public class RegularityItem
    {
        public string Label { get; set; } = "";
        public double StdDev { get; set; }
        public string Color { get; set; } = "#FFFFFF";
    }
}
