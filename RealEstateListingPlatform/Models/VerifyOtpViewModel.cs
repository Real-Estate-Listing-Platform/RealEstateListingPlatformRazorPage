using System.ComponentModel.DataAnnotations;

namespace RealEstateListingPlatform.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        [Display(Name = "Verification Code")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
