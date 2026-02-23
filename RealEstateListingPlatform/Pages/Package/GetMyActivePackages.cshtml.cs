using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class GetMyActivePackagesModel : PageModel
    {
        private readonly IPackageService _packageService;

        public GetMyActivePackagesModel(IPackageService packageService)
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
            var result = await _packageService.GetActiveUserPackagesAsync(userId);

            if (!result.Success)
                return new JsonResult(new { success = false, message = result.Message });

            var packages = result.Data?.Select(p => new
            {
                id = p.Id,
                name = p.Package.Name,
                type = p.Package.PackageType,
                photoLimit = p.Package.PhotoLimit,
                allowVideo = p.VideoAvailable,
                remainingListings = p.RemainingListings,
                remainingPhotos = p.RemainingPhotos,
                expiresAt = p.ExpiresAt?.ToString("yyyy-MM-dd")
            });

            return new JsonResult(new { success = true, data = packages });
        }
    }
}
