namespace MauiApp1.Models
{
    /// <summary>
    /// Represents a Point of Interest (POI) on the map.
    /// </summary>
    public class PointOfInterest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Distance { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HasAudio { get; set; }
        public string AudioStatus { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
