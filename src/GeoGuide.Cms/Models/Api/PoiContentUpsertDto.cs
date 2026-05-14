using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.Api;

public class PoiContentUpsertDto
{
    [Required, StringLength(10)]
    public string LanguageCode { get; init; } = "vi";

    [Required]
    public string ContentType { get; init; } = "TtsScript";

    [Url]
    public string? AudioUrl { get; init; }

    public string? TtsContent { get; init; }
}
