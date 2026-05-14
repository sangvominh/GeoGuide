namespace GeoGuide.Cms.Models.Api;

public class OwnerPoiCreateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TriggerRadiusMeters { get; set; } = 80;
    public int CooldownMinutes { get; set; } = 15;
    public string CategoryKey { get; set; } = "attraction";
    public string CategoryLabel { get; set; } = "Tham quan";
    public string ImageUrl { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
}

public class OwnerPoiUpdateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TriggerRadiusMeters { get; set; } = 80;
    public int CooldownMinutes { get; set; } = 15;
    public string CategoryKey { get; set; } = "attraction";
    public string CategoryLabel { get; set; } = "Tham quan";
    public string ImageUrl { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
}
