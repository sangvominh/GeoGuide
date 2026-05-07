using SQLite;

namespace MauiApp1.Services;

[Table("offline_logs")]
public sealed class LocalOfflineLogRecord
{
    [PrimaryKey]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [Column("poi_id")]
    public string? PoiId { get; set; }

    [Column("event_type")]
    public int EventType { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("timestamp_utc")]
    public string TimestampUtcIso { get; set; } = string.Empty;

    [Column("sync_status")]
    public int SyncStatus { get; set; }
}
