using SQLite;

namespace MauiApp1.Services;

[Table("pois")]
public sealed class LocalPoiRecord
{
    [PrimaryKey]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("trigger_radius_meters")]
    public double TriggerRadiusMeters { get; set; }

    [Column("cooldown_minutes")]
    public int CooldownMinutes { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [Column("category_key")]
    public string CategoryKey { get; set; } = string.Empty;

    [Column("category_label")]
    public string CategoryLabel { get; set; } = string.Empty;

    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("map_url")]
    public string MapUrl { get; set; } = string.Empty;

    [Column("audio_url")]
    public string AudioUrl { get; set; } = string.Empty;

    [Column("tts_script")]
    public string TtsScript { get; set; } = string.Empty;

    [Column("language_code")]
    public string LanguageCode { get; set; } = "vi-VN";

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("updated_at")]
    public string UpdatedAtIso { get; set; } = string.Empty;
}
