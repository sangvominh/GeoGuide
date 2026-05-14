using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Security;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/admin/auth")]
[Authorize]
public class AdminAuthV1Controller(
    UserManager<ApplicationUser> userManager,
    CmsAccessService accessService,
    ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var scope = await accessService.GetScopeAsync();
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new AdminAuthMeResponseDto
        {
            DisplayName = user.DisplayName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Permissions = scope.Permissions
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpPost("register-owner")]
    [Authorize(Roles = CmsRoles.SystemAdmin)]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerRequestDto dto)
    {
        var slugExists = await dbContext.PoiTenants.AnyAsync(t => t.Slug == dto.TenantSlug);
        if (slugExists)
        {
            return Conflict(new { Message = "Tenant slug already exists." });
        }

        var tenant = new PoiTenant
        {
            Id = Guid.NewGuid(),
            Name = dto.TenantName,
            Slug = dto.TenantSlug,
            IsActive = true,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PoiTenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            TenantId = tenant.Id
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await userManager.AddToRoleAsync(user, CmsRoles.PoiTenant);

        return Ok();
    }
}
