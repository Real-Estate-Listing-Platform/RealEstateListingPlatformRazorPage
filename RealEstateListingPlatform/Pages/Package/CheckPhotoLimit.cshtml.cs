using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class CheckPhotoLimitModel : PageModel
    {
        private readonly IPackageService _packageService;

        public CheckPhotoLimitModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync(Guid? listingId)
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.GetAvailablePhotosForListingAsync(userId, listingId);

            return new JsonResult(new
            {
                success = result.Success,
                photoLimit = result.Data,
                message = result.Message
            });
        }
    }
}
