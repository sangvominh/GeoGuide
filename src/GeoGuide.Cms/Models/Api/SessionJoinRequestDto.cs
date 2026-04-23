using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.Api;

public class SessionJoinRequestDto
{
    [Required, StringLength(120)]
    public string SessionToken { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string ClientType { get; set; } = "mobile";

    [Required, StringLength(20)]
    public string AccessMode { get; set; } = "trial";

    public DateTimeOffset JoinedAt { get; set; }
}
