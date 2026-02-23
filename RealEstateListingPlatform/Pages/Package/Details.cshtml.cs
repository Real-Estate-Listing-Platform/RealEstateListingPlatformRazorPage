using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IPackageService _packageService;

        public DetailsModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _packageService.GetPackageByIdAsync(Id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Package not found";
                return RedirectToPage("/Package/Index");
            }

            // For AJAX calls, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = true, data = result.Data });
            }

            // For regular calls, redirect to purchase page
            return RedirectToPage("/Package/Purchase", new { id = Id });
        }
    }
}
