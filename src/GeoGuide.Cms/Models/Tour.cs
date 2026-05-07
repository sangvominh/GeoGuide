using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class Tour
{
    public Guid Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Url]
    public string ThumbnailUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TourPoiMapping> PoiMappings { get; set; } = [];
}
