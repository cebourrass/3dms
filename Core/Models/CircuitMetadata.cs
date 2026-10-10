namespace Analyzer.Models
{
    /// <summary>
    /// Circuit disponible dans la bibliothèque de cartes (.map), avec son point de départ.
    /// </summary>
    public class CircuitMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public GpsPoint? StartPoint { get; set; }
        public override string ToString() => Name;
    }
}
