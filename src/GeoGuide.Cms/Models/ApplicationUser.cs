using Microsoft.AspNetCore.Identity;

namespace GeoGuide.Cms.Models;

public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; }

    public PoiTenant? Tenant { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}
