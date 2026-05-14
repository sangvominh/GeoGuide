using GeoGuide.Cms.Models.Api.V1;

namespace GeoGuide.Cms.Services;

public interface IAiAdvisorService
{
    Task<EnhanceDescriptionResponse> EnhanceDescriptionAsync(EnhanceDescriptionRequest request, string userId);
}
