using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer.Services;

namespace Analyzer.Models
{
    /// <summary>
    /// Représente l'intégralité d'une session de roulage (.ra1)
    /// </summary>
    public class SessionData
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        
        // Données complètes
        public List<TelemetryPoint> AllPoints { get; set; } = new();
        public List<LapData> Laps { get; set; } = new();
        
        // Carte associée
        public TrackMap? CircuitMap { get; set; }
        public string MapFilePath { get; set; } = string.Empty;
        public int PartialCount { get; set; }

        // Records de la session
        public string BestLapTime { get; set; } = "--:--.--";
        public string IdealTime { get; set; } = "--:--.--"; // Nouveau
        public double MaxSpeed { get; set; }
        public double MaxLeanLeft { get; set; }
        public double MaxLeanRight { get; set; }
        public float MaxAccel { get; set; }
        public float MaxDecel { get; set; }

        // Méta-données session (Statistiques)
        public string Event { get; set; } = string.Empty;
        public string Pilot { get; set; } = string.Empty;
        public string Vehicle { get; set; } = string.Empty;
        public string TrackConditions { get; set; } = "Dry";
        public double TrackTemperature { get; set; } = 20;
        public string Tires { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
