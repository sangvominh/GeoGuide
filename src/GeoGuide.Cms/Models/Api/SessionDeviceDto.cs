namespace GeoGuide.Cms.Models.Api;

public class SessionDeviceDto
{
    public string DeviceId { get; init; } = string.Empty;
    public string ClientType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
    public int ListenCount { get; init; }
    public int TotalDurationSeconds { get; init; }
    public string LastPoiName { get; init; } = string.Empty;
}
