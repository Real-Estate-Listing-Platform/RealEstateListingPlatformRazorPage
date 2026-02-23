using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class ApplyToListingModel : PageModel
    {
        private readonly IPackageService _packageService;

        public ApplyToListingModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnPostAsync(Guid userPackageId, Guid listingId)
        {
            var userId = GetCurrentUserId();

            var applyDto = new ApplyPackageDto
            {
                UserPackageId = userPackageId,
                ListingId = listingId
            };

            var result = await _packageService.ApplyPackageToListingAsync(userId, applyDto);

            if (!result.Success)
            {
                return new JsonResult(new { success = false, message = result.Message });
            }

            return new JsonResult(new { success = true, message = result.Message });
        }
    }
}
