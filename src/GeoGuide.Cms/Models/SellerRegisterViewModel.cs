using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class SellerRegisterViewModel
{
    [Required]
    [StringLength(200)]
    public string TenantName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
    public string TenantSlug { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 10)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string OwnerDisplayName { get; set; } = string.Empty;
}
