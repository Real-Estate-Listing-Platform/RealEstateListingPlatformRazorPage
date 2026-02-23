using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.RegisterAsync(Input.FullName, Input.Email, Input.Password, Input.PhoneNumber);

            if (result.Success)
            {
                return RedirectToPage("./VerifyEmail", new { email = Input.Email });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return Page();
        }
    }
}
