using System.Globalization;
using System.Text.Json.Serialization;

namespace MauiApp1.Models;

/// <summary>
/// Mobile POI model aligned with the shared MVP contract.
/// </summary>
public class PointOfInterest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TriggerRadiusMeters { get; set; } = 80;
    public int CooldownMinutes { get; set; } = 5;
    public int Priority { get; set; } = 1;
    public string CategoryKey { get; set; } = "attraction";
    public string CategoryLabel { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string TtsScript { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "vi-VN";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonIgnore]
    public double DistanceMeters { get; set; } = double.MaxValue;

    [JsonIgnore]
    public string DistanceDisplay => FormatDistance(DistanceMeters);

    [JsonIgnore]
    public string IconGlyph => CategoryKey switch
    {
        "food" => "\ue56c",
        "cafe" => "\ue541",
        "park" => "\ueb2f",
        "play" => "\uea44",
        "theatre" => "\ue03d",
        "attraction" => "\ue56b",
        _ => "\ue55f"
    };

    [JsonIgnore]
    public string NarrationText =>
        !string.IsNullOrWhiteSpace(TtsScript)
            ? TtsScript
            : string.Join(
                " ",
                new[] { Name, Description }
                    .Where(static value => !string.IsNullOrWhiteSpace(value)));

    [JsonIgnore]
    public string Category
    {
        get => CategoryLabel;
        set => CategoryLabel = value;
    }

    [JsonIgnore]
    public string Distance
    {
        get => DistanceDisplay;
        set
        {
        }
    }

    [JsonIgnore]
    public bool HasAudio => !string.IsNullOrWhiteSpace(AudioUrl) || !string.IsNullOrWhiteSpace(TtsScript);

    [JsonIgnore]
    public string AudioStatus => HasAudio ? "AUDIO" : "NONE";

    [JsonIgnore]
    public double Rating { get; set; }

    public void UpdateDistanceFrom(Location location)
    {
        DistanceMeters = Location.CalculateDistance(
            location.Latitude,
            location.Longitude,
            Latitude,
            Longitude,
            DistanceUnits.Kilometers) * 1000d;
    }

    public void UpdateDistanceFrom(double latitude, double longitude)
    {
        DistanceMeters = Location.CalculateDistance(
            latitude,
            longitude,
            Latitude,
            Longitude,
            DistanceUnits.Kilometers) * 1000d;
    }

    private static string FormatDistance(double distanceMeters)
    {
        if (double.IsNaN(distanceMeters) || double.IsInfinity(distanceMeters) || distanceMeters == double.MaxValue)
        {
            return "--";
        }

        if (distanceMeters < 1000)
        {
            return $"{Math.Round(distanceMeters, 0).ToString(CultureInfo.InvariantCulture)}m";
        }

        var kilometers = distanceMeters / 1000d;
        return $"{kilometers.ToString("0.0", CultureInfo.InvariantCulture)}km";
    }
}
