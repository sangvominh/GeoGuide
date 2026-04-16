using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class PoiTenant
{
    public static readonly Guid DemoTenantId = Guid.Parse("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75");
    public static readonly Guid VinhKhanhTenantId = Guid.Parse("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0");

    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
