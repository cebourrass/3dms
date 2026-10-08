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
    }
}
