namespace GeoGuide.Cms.Models.Api.V1;

public class EnhanceDescriptionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
}
