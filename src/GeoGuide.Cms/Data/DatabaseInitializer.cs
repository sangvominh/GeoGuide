using GeoGuide.Cms.Models;
using GeoGuide.Cms.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GeoGuide.Cms.Data;

public static class DatabaseInitializer
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        await SeedIdentityAsync(scope.ServiceProvider);
    }

    private static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var options = services.GetRequiredService<IOptions<CmsBootstrapOptions>>().Value;

        foreach (var roleName in CmsRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await EnsureTenantUserAsync(userManager, options);
        await EnsureAdminUserAsync(userManager, options);
    }

    private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager, CmsBootstrapOptions options)
    {
        var admin = await userManager.FindByEmailAsync(options.Admin.Email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = options.Admin.Email,
                Email = options.Admin.Email,
                DisplayName = options.Admin.DisplayName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, options.Admin.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unable to seed admin user: {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, CmsRoles.SystemAdmin))
        {
            await userManager.AddToRoleAsync(admin, CmsRoles.SystemAdmin);
        }
    }

    private static async Task EnsureTenantUserAsync(UserManager<ApplicationUser> userManager, CmsBootstrapOptions options)
    {
        var tenantUser = await userManager.FindByEmailAsync(options.Tenant.Email);
        if (tenantUser is null)
        {
            tenantUser = new ApplicationUser
            {
                UserName = options.Tenant.Email,
                Email = options.Tenant.Email,
                DisplayName = options.Tenant.DisplayName,
                EmailConfirmed = true,
                TenantId = options.Tenant.TenantId
            };

            var result = await userManager.CreateAsync(tenantUser, options.Tenant.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unable to seed tenant user: {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }
        }
        else if (tenantUser.TenantId != options.Tenant.TenantId)
        {
            tenantUser.TenantId = options.Tenant.TenantId;
            await userManager.UpdateAsync(tenantUser);
        }

        if (!await userManager.IsInRoleAsync(tenantUser, CmsRoles.PoiTenant))
        {
            await userManager.AddToRoleAsync(tenantUser, CmsRoles.PoiTenant);
        }
    }
}
