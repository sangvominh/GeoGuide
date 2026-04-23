using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GeoGuide.Cms.Models;

public class PoiAudio
{
    public Guid Id { get; set; }

    public Guid PoiId { get; set; }

    public Poi? Poi { get; set; }

    [Display(Name = "Mã ngôn ngữ")]
    [Required, StringLength(10)]
    public string LanguageCode { get; set; } = "vi";

    [Display(Name = "Loại nội dung phát")]
    public PoiContentType ContentType { get; set; } = PoiContentType.TtsScript;

    [Display(Name = "Đường dẫn file audio")]
    [Url]
    public string? AudioUrl { get; set; }

    [NotMapped]
    [Display(Name = "Tệp audio tải lên")]
    public IFormFile? UploadFile { get; set; }

    [Display(Name = "Nội dung TTS")]
    public string? TtsContent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
}
