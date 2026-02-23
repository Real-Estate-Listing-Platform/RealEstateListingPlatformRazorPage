using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IAuthService _authService;

        public VerifyEmailModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public VerifyOtpViewModel Input { get; set; }

        public IActionResult OnGet(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("./Register");
            }
            Input = new VerifyOtpViewModel { Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.VerifyOtpAsync(Input.Email, Input.OtpCode);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage("./Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendOtpAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required to resend OTP.";
                return RedirectToPage("./Register");
            }

            var result = await _authService.ResendOtpAsync(email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage("./VerifyEmail", new { email = email });
        }
    }
}
