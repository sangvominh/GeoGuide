using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Route("[controller]")]
public class OwnerPortalController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View((List<Poi>?)null);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null || user.TenantId == null || !await userManager.IsInRoleAsync(user, CmsRoles.PoiTenant))
        {
            return View((List<Poi>?)null);
        }

        var pois = await dbContext.Pois
            .Where(p => p.TenantId == user.TenantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(pois);
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(SellerRegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await dbContext.PoiTenants.AnyAsync(t => t.Slug == model.TenantSlug))
        {
            ModelState.AddModelError("TenantSlug", "Tenant slug already exists.");
            return View(model);
        }

        if (await userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError("Email", "Email already registered.");
            return View(model);
        }

        var tenant = new PoiTenant
        {
            Id = Guid.NewGuid(),
            Name = model.TenantName,
            Slug = model.TenantSlug,
            IsActive = true,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PoiTenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.OwnerDisplayName,
            TenantId = tenant.Id,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, CmsRoles.PoiTenant);
            return Redirect("/Account/Login?registered=true");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [Authorize(Roles = CmsRoles.PoiTenant)]
    [HttpGet("Pois/Create")]
    public IActionResult CreatePoi()
    {
        return View();
    }

    [Authorize(Roles = CmsRoles.PoiTenant)]
    [HttpPost("Pois/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePoi(PoiCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user?.TenantId == null) return Unauthorized();

        var poi = new Poi
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
            Name = model.Name,
            Description = model.Description,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            ImageUrl = model.ImageUrl,
            MapUrl = model.MapUrl,
            ApprovalStatus = PoiApprovalStatus.PendingApproval,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Pois.Add(poi);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = CmsRoles.PoiTenant)]
    [HttpGet("Pois/Edit/{id:guid}")]
    public async Task<IActionResult> EditPoi(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user?.TenantId == null) return Unauthorized();

        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == user.TenantId);
        if (poi == null) return NotFound();

        var model = new PoiEditViewModel
        {
            Id = poi.Id,
            Name = poi.Name,
            Description = poi.Description,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            ImageUrl = poi.ImageUrl,
            MapUrl = poi.MapUrl
        };

        return View(model);
    }

    [Authorize(Roles = CmsRoles.PoiTenant)]
    [HttpPost("Pois/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPoi(Guid id, PoiEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user?.TenantId == null) return Unauthorized();

        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == user.TenantId);
        if (poi == null) return NotFound();

        poi.Name = model.Name;
        poi.Description = model.Description;
        poi.Latitude = model.Latitude;
        poi.Longitude = model.Longitude;
        poi.ImageUrl = model.ImageUrl;
        poi.MapUrl = model.MapUrl;
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
