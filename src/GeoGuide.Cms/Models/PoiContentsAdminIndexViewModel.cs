namespace GeoGuide.Cms.Models;

public class PoiContentsAdminIndexViewModel
{
    public Poi Poi { get; init; } = null!;

    public IReadOnlyList<PoiAudio> Contents { get; init; } = [];

    public bool IsSystemAdmin { get; init; }
}
