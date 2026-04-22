using System;

namespace Analyzer.Models
{
    public class UserSettings
    {
        // Fenêtre
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public string WindowState { get; set; } = "Normal";

        // Visibilité des panneaux
        public bool IsLapsVisible { get; set; } = true;
        public bool IsSessionInfoVisible { get; set; } = true;
        public bool IsMapVisible { get; set; } = true;
        public bool IsChartsVisible { get; set; } = true;
        public bool IsExplorerVisible { get; set; } = true;

        // Styles des courbes
        public string SpeedColor { get; set; } = "#10b981";
        public float SpeedThickness { get; set; } = 1.8f;
        public string AngleColor { get; set; } = "#fbbf24";
        public float AngleThickness { get; set; } = 1.8f;
        public string AngleRightColor { get; set; } = "#f59e0b";
        public float AngleRightThickness { get; set; } = 1.8f;
        public string AccelColor { get; set; } = "#8b5cf6";
        public float AccelThickness { get; set; } = 1.8f;
        public string RefColor { get; set; } = "#ffffff";
        public float RefThickness { get; set; } = 1.5f;

        // Épaisseurs de comparaison
        public float CompFastThickness { get; set; } = 1.5f;
        public float CompSlowThickness { get; set; } = 0.5f;

        // Dernière session
        public string? LastFilePath { get; set; }
    }
}
