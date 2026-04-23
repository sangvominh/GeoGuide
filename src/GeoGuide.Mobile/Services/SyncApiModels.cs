namespace MauiApp1.Services;

public sealed class SyncPayload
{
    public DateTimeOffset ServerTime { get; init; }
    public IReadOnlyList<SyncPoiDto> Pois { get; init; } = [];
}

public sealed class SyncPoiDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double TriggerRadiusMeters { get; init; }
    public int CooldownMinutes { get; init; }
    public int Priority { get; init; }
    public string CategoryKey { get; init; } = string.Empty;
    public string CategoryLabel { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string MapUrl { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<SyncPoiContentDto> Contents { get; init; } = [];
}

public sealed class SyncPoiContentDto
{
    public Guid Id { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string? AudioUrl { get; init; }
    public string? TtsContent { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SessionJoinRequest
{
    public string SessionToken { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string ClientType { get; init; } = "mobile";
    public string AccessMode { get; init; } = "trial";
    public DateTimeOffset JoinedAt { get; init; }
}

public sealed class SessionJoinResponse
{
    public string SessionToken { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string ClientType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset ServerTime { get; init; }
}
