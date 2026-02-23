using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class VerifyResetOtpModel : PageModel
    {
        private readonly IAuthService _authService;

        public VerifyResetOtpModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public VerifyOtpViewModel Input { get; set; }

        public IActionResult OnGet(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToPage("./ForgotPassword");
            Input = new VerifyOtpViewModel { Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _authService.VerifyResetOtpAsync(Input.Email, Input.OtpCode);

            if (result.Success)
            {
                return RedirectToPage("./ResetPassword", new { email = Input.Email, token = result.Token });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendForgotPasswordOtpAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required to resend OTP.";
                return RedirectToPage("./ForgotPassword");
            }

            var result = await _authService.ForgotPasswordAsync(email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage("./VerifyResetOtp", new { email = email });
        }
    }
}
