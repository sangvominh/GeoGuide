using GeoGuide.Cms.Models;
using GeoGuide.Cms.Security;
using Microsoft.AspNetCore.Identity;

namespace GeoGuide.Cms.Services;

public class CmsAccessService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
{
    public async Task<CmsAccessScope> GetScopeAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return new CmsAccessScope(false, null, []);
        }

        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return new CmsAccessScope(false, null, []);
        }

        var isSystemAdmin = await userManager.IsInRoleAsync(user, CmsRoles.SystemAdmin);
        var isTenant = await userManager.IsInRoleAsync(user, CmsRoles.PoiTenant) || user.TenantId.HasValue;

        var permissions = new List<string>();

        if (isSystemAdmin)
        {
            permissions.AddRange(new[] { "content", "audio", "admin", "owner", "ai_advisor", "localization", "maps", "analytics", "tours" });
        }
        else if (isTenant)
        {
            permissions.AddRange(new[] { "owner", "analytics" });
        }

        return new CmsAccessScope(isSystemAdmin, user.TenantId, permissions.ToArray());
    }
}
