using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.AiAdvisor;

public class AiAdvisorViewModel
{
    [Required]
    [Display(Name = "POI Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Price Range")]
    public string PriceRange { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Current Description")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    public string? EnhancedDescription { get; set; }
    public string? Provider { get; set; }
    public bool IsEnhanced => !string.IsNullOrEmpty(EnhancedDescription);
    public string? ErrorMessage { get; set; }
}
