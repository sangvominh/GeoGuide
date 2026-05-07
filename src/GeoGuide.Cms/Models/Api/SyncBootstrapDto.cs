namespace GeoGuide.Cms.Models.Api;

public class SyncBootstrapDto
{
    public DateTimeOffset ServerTime { get; init; }

    public IReadOnlyList<PoiDto> Pois { get; init; } = [];

    public IReadOnlyList<TourDto> Tours { get; init; } = [];
}
