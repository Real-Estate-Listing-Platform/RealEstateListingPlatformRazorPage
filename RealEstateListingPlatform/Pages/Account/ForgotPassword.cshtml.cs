using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthService _authService;

        public ForgotPasswordModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _authService.ForgotPasswordAsync(Input.Email);

            if (result.Success)
            {
                return RedirectToPage("./VerifyResetOtp", new { email = Input.Email });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return Page();
        }
    }
}
