using System;

namespace Analyzer.Models
{
    /// <summary>
    /// Représente un point de données télémétriques extrait d'une session 3DMS.
    /// </summary>
    public class TelemetryPoint
    {
        /// <summary>
        /// Temps écoulé depuis le début de la session en millisecondes.
        /// </summary>
        public uint Time { get; set; }

        /// <summary>
        /// Longitude GPS du point.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Latitude GPS du point.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Vitesse du véhicule en km/h.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Angle d'inclinaison du véhicule en degrés.
        /// </summary>
        public float LeanAngle { get; set; }

        /// <summary>
        /// Accélération longitudinale en G.
        /// </summary>
        public float Acceleration { get; set; }
    }
}
