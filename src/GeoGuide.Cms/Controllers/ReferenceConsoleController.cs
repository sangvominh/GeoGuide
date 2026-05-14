using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class ReferenceConsoleController(
    AudioLocalizationService audioLocalizationService,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new ReferenceConsoleIndexViewModel();
        
        // Populate initial data
        model.Localization.WarmupStatus = audioLocalizationService.GetWarmupStatus(model.Localization.LanguageCode);
        PopulateMapsStatus(model.Maps);

        if (TempData["LocalizationMessage"] != null)
        {
            model.Localization.ResultMessage = TempData["LocalizationMessage"]?.ToString();
        }
        if (TempData["AudioMessage"] != null)
        {
            model.Audio.ResultMessage = TempData["AudioMessage"]?.ToString();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> WarmupLocalization(ReferenceConsoleIndexViewModel input)
    {
        var lang = input.Localization.LanguageCode ?? "vi";
        
        var request = new LocalizationWarmupRequestDto { LanguageCode = lang };
        await audioLocalizationService.WarmupAsync(request, HttpContext.RequestAborted);
        
        TempData["LocalizationMessage"] = $"Warmup task for '{lang}' has been triggered.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateAudio(ReferenceConsoleIndexViewModel input)
    {
        var text = input.Audio.TextToSpeak;
        var lang = input.Audio.LanguageCode;

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(lang))
        {
            TempData["AudioMessage"] = "Text and Language Code are required.";
            return RedirectToAction(nameof(Index));
        }

        var request = new TtsRequestDto { Text = text, LanguageCode = lang };
        var asset = await audioLocalizationService.GenerateTtsAsync(request, HttpContext.RequestAborted);

        var model = new ReferenceConsoleIndexViewModel();
        model.Localization.WarmupStatus = audioLocalizationService.GetWarmupStatus(model.Localization.LanguageCode);
        PopulateMapsStatus(model.Maps);
        
        model.Audio = input.Audio;
        model.Audio.AudioUrl = asset.AudioUrl;
        model.Audio.Provider = asset.Provider;
        model.Audio.GeneratedAt = asset.UpdatedAt;
        model.Audio.ResultMessage = "Audio generated successfully via local-demo provider.";

        return View("Index", model);
    }

    private void PopulateMapsStatus(MapsPanelViewModel mapsModel)
    {
        var webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        var mapsRoot = Path.Combine(webRoot, "static", "maps");
        
        var packsRoot = Path.Combine(mapsRoot, "packs");
        var stylesRoot = Path.Combine(mapsRoot, "styles");
        var fontsRoot = Path.Combine(mapsRoot, "fonts");

        mapsModel.PackCount = Directory.Exists(packsRoot) ? Directory.GetDirectories(packsRoot).Length : 0;
        
        mapsModel.StyleCount = Directory.Exists(stylesRoot) ? 
            Directory.GetFiles(stylesRoot, "*", SearchOption.AllDirectories)
                .Count(f => !Path.GetFileName(f).StartsWith(".") && Path.GetFileName(f) != "README.md") : 0;
                
        mapsModel.FontStackCount = Directory.Exists(fontsRoot) ? Directory.GetDirectories(fontsRoot).Length : 0;
        
        if (mapsModel.PackCount == 0 && mapsModel.StyleCount == 0 && mapsModel.FontStackCount == 0)
        {
            mapsModel.ResultMessage = "No offline map assets found. Please follow the placement instructions.";
        }
        else
        {
            mapsModel.ResultMessage = "Offline map assets are present.";
        }
    }
}
