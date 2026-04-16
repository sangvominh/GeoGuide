using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GeoGuide.Cms.Models;

public class PoiAudio
{
    public Guid Id { get; set; }

    public Guid PoiId { get; set; }

    public Poi? Poi { get; set; }

    [Required, StringLength(10)]
    public string LanguageCode { get; set; } = "vi";

    public PoiContentType ContentType { get; set; } = PoiContentType.TtsScript;

    [Url]
    public string? AudioUrl { get; set; }

    [NotMapped]
    public IFormFile? UploadFile { get; set; }

    public string? TtsContent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
}
