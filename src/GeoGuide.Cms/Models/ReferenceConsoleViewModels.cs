using System.ComponentModel.DataAnnotations;
using GeoGuide.Cms.Models.Api;

namespace GeoGuide.Cms.Models;

public class ReferenceConsoleIndexViewModel
{
    public LocalizationPanelViewModel Localization { get; set; } = new();
    public AudioPanelViewModel Audio { get; set; } = new();
    public MapsPanelViewModel Maps { get; set; } = new();
}

public class LocalizationPanelViewModel
{
    [Required, Display(Name = "Language Code")]
    public string LanguageCode { get; set; } = "vi";
    
    public LocalizationTaskStatusDto? WarmupStatus { get; set; }
    public string? ResultMessage { get; set; }
}

public class AudioPanelViewModel
{
    [Required, Display(Name = "Text to Speak")]
    public string TextToSpeak { get; set; } = "Xin chào, đây là demo âm thanh.";
    
    [Required, Display(Name = "Language Code")]
    public string LanguageCode { get; set; } = "vi";
    
    public string? AudioUrl { get; set; }
    public string? Provider { get; set; }
    public DateTimeOffset? GeneratedAt { get; set; }
    public string? ResultMessage { get; set; }
}

public class MapsPanelViewModel
{
    public int PackCount { get; set; }
    public int StyleCount { get; set; }
    public int FontStackCount { get; set; }
    
    public string AssetPlacementInstructions { get; set; } = "Place packs under wwwroot/static/maps/packs/{version}/{file}.pmtiles\nStyles under wwwroot/static/maps/styles/\nFonts under wwwroot/static/maps/fonts/";
    public string? ResultMessage { get; set; }
}
