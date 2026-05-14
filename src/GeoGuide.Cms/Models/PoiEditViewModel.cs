using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class PoiEditViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    [Url]
    public string MapUrl { get; set; } = string.Empty;
}
