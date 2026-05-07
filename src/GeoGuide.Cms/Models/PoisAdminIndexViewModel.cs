using Microsoft.AspNetCore.Mvc.Rendering;

namespace GeoGuide.Cms.Models;

public class PoisAdminIndexViewModel
{
    public IReadOnlyList<Poi> Pois { get; init; } = [];

    public Guid? SelectedTenantId { get; init; }

    public bool IsSystemAdmin { get; init; }

    public IReadOnlyList<SelectListItem> TenantOptions { get; init; } = [];
}
