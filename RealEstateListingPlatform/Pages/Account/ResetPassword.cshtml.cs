using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAuthService _authService;

        public ResetPasswordModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public ResetPasswordViewModel Input { get; set; }

        public IActionResult OnGet(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return RedirectToPage("./ForgotPassword");
            Input = new ResetPasswordViewModel { Email = email, Token = token };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _authService.ResetPasswordAsync(Input.Email, Input.Token, Input.NewPassword);

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
    }
}
