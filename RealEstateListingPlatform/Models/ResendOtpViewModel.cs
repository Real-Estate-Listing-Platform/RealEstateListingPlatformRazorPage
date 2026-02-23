using System.ComponentModel.DataAnnotations;

namespace RealEstateListingPlatform.Models
{
    public class ResendOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}