namespace GeoGuide.Cms.Models;

public class TourPoiMapping
{
    public Guid TourId { get; set; }

    public Tour? Tour { get; set; }

    public Guid PoiId { get; set; }

    public Poi? Poi { get; set; }

    public int OrderIndex { get; set; }
}
