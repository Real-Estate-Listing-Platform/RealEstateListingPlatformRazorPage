using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class BoostModel : PageModel
    {
        private readonly IPackageService _packageService;

        public BoostModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnPostAsync(Guid listingId, Guid? userPackageId, int boostDays = 7)
        {
            var userId = GetCurrentUserId();

            var boostDto = new BoostListingDto
            {
                ListingId = listingId,
                UserPackageId = userPackageId,
                BoostDays = boostDays
            };

            var result = await _packageService.BoostListingAsync(userId, boostDto);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToPage("/Lister/Details", new { id = listingId });
            }

            TempData["Success"] = "Listing boosted successfully!";
            return RedirectToPage("/Lister/Details", new { id = listingId });
        }
    }
}
