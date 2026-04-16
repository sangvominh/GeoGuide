using Microsoft.AspNetCore.Mvc.Rendering;

namespace GeoGuide.Cms.Models;

public class TourPoiMappingsAdminViewModel
{
    public Tour Tour { get; init; } = null!;

    public IReadOnlyList<TourPoiMapping> Mappings { get; init; } = [];

    public IReadOnlyList<SelectListItem> AvailablePois { get; init; } = [];
}
