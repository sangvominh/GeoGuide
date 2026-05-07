namespace GeoGuide.Cms.Data;

public class CmsBootstrapOptions
{
    public const string SectionName = "CmsBootstrap";

    public BootstrapUserOptions Admin { get; set; } = new();

    public BootstrapTenantOptions Tenant { get; set; } = new();

    public BootstrapTenantOptions VinhKhanhTenant { get; set; } = new()
    {
        Email = "tenant-vinhkhanh@geoguide.local",
        Password = "ChangeThis_VinhKhanh123!",
        DisplayName = "Vinh Khanh Tenant Manager",
        TenantId = Models.PoiTenant.VinhKhanhTenantId,
        TenantName = "Vinh Khanh Food Street",
        TenantSlug = "vinh-khanh-food-street"
    };
}

public class BootstrapUserOptions
{
    public string Email { get; set; } = "admin@geoguide.local";

    public string Password { get; set; } = "ChangeThis_Admin123!";

    public string DisplayName { get; set; } = "System Admin";
}

public class BootstrapTenantOptions : BootstrapUserOptions
{
    public Guid TenantId { get; set; } = Models.PoiTenant.DemoTenantId;

    public string TenantName { get; set; } = "Demo POI Tenant";

    public string TenantSlug { get; set; } = "demo-poi-tenant";
}
