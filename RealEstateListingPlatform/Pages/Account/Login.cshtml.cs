using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public LoginModel(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Email, Input.Password);

            if (result.Success)
            {
                var user = await _userService.GetUserByEmail(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Login failed: user not found.");
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return Page();
        }
    }
}
