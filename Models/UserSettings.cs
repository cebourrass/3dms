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
        public bool IsSessionInfoVisible { get; set; } = false;
        public bool IsMapVisible { get; set; } = true;
        public bool IsChartsVisible { get; set; } = true;
        public bool IsExplorerVisible { get; set; } = true;
        
        public bool ShowSpeed { get; set; } = true;
        public bool ShowAngleLeft { get; set; } = false;
        public bool ShowAngleRight { get; set; } = false;
        public bool ShowAccel { get; set; } = false;
        public bool ShowDecel { get; set; } = false;
        public bool ShowReference { get; set; } = false;
        public bool ShowAccelMapGradient { get; set; } = true;
        public double AccelGradientRange { get; set; } = 1.2;
        public bool AutoAccelGradientScaling { get; set; } = false;
        public bool AutoCenterMap { get; set; } = true;
        public bool ShowCorruptionWarning { get; set; } = false;

        // Styles des courbes
        public string SpeedColor { get; set; } = "#10b981";
        public float SpeedThickness { get; set; } = 1.4f;
        public string AngleColor { get; set; } = "#FFFF00";
        public float AngleThickness { get; set; } = 1.4f;
        public string AngleRightColor { get; set; } = "#800040";
        public float AngleRightThickness { get; set; } = 1.5f;
        public string AccelColor { get; set; } = "#00FF00";
        public float AccelThickness { get; set; } = 1.6f;
        public string DecelColor { get; set; } = "#ef4444";
        public float DecelThickness { get; set; } = 1.4f;
        public string RefColor { get; set; } = "#ffffff";
        public float RefThickness { get; set; } = 1.5f;

        // Épaisseurs de comparaison
        public float CompFastThickness { get; set; } = 1.5f;
        public float CompSlowThickness { get; set; } = 0.5f;

        // Lissage des courbes (Interpolation)
        // 1: Aucun, 2: Bas (window=2), 3: Moyen (window=4), 5: Haut (window=7)
        public int SpeedSmoothing { get; set; } = 3;
        public int AngleSmoothing { get; set; } = 3;
        public int AccelSmoothing { get; set; } = 8;
        public int GpsSmoothing { get; set; } = 3;
        public string Language { get; set; } = "English";

        // Fréquence d'interpolation (ms)
        public double InterpolationStepMs { get; set; } = 20.0;

        // Paramètres de la carte
        public double MapTrajectoryThickness { get; set; } = 0.6;
        public double MapCursorSize { get; set; } = 3.0;

        // Seuils de régularité
        public double RegularityThresholdExcellent { get; set; } = 0.10;
        public double RegularityThresholdMedium { get; set; } = 0.30;

        // Dernière session
        public string? LastFilePath { get; set; }
        public string? SelectedPilotProfileName { get; set; }
    }
}
