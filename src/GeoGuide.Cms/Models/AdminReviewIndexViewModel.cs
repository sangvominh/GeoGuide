namespace GeoGuide.Cms.Models;

public class AdminReviewIndexViewModel
{
    public IReadOnlyList<Poi> PendingPois { get; set; } = [];
    public IReadOnlyList<Poi> RecentPois { get; set; } = [];
}
