using System.Security.Claims;
using GeoGuide.Cms.Models.AiAdvisor;
using GeoGuide.Cms.Models.Api.V1;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class AiAdvisorController : Controller
{
    private readonly IAiAdvisorService _aiAdvisorService;

    public AiAdvisorController(IAiAdvisorService aiAdvisorService)
    {
        _aiAdvisorService = aiAdvisorService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new AiAdvisorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AiAdvisorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                         ?? "anonymous";

            var request = new EnhanceDescriptionRequest
            {
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                Address = model.Address,
                PriceRange = model.PriceRange
            };

            var response = await _aiAdvisorService.EnhanceDescriptionAsync(request, userId);

            model.EnhancedDescription = response.EnhancedDescription;
            model.Provider = response.Provider;
        }
        catch (InvalidOperationException ex)
        {
            model.ErrorMessage = ex.Message; // E.g. usage limit reached
        }
        catch (Exception)
        {
            model.ErrorMessage = "An error occurred while enhancing the description. Please try again later.";
        }

        return View(model);
    }
}
