namespace GeoGuide.Cms.Models.Api;

public class SessionJoinResponseDto
{
    public string SessionToken { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string ClientType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset ServerTime { get; init; }
}
