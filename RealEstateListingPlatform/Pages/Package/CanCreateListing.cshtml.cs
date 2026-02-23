using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class CanCreateListingModel : PageModel
    {
        private readonly IPackageService _packageService;

        public CanCreateListingModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.CanUserCreateListingAsync(userId);

            return new JsonResult(new
            {
                success = result.Success,
                canCreate = result.Success && result.Data,
                message = result.Message
            });
        }
    }
}
