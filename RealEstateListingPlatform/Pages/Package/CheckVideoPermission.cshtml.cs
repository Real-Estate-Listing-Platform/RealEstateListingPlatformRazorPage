using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class CheckVideoPermissionModel : PageModel
    {
        private readonly IPackageService _packageService;

        public CheckVideoPermissionModel(IPackageService packageService)
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

            // If listing exists, check its AllowVideo property
            if (listingId.HasValue)
            {
                // Note: Would need IListingService injected to check listing
                // For now, check user's active packages
            }

            // Check active video packages
            var packages = await _packageService.GetActiveUserPackagesAsync(userId);
            if (packages.Success && packages.Data != null)
            {
                var hasVideoPackage = packages.Data.Any(p =>
                    p.Package.PackageType == "VIDEO_UPLOAD" &&
                    p.VideoAvailable &&
                    p.Status == "Active");

                return new JsonResult(new
                {
                    allowed = hasVideoPackage,
                    message = hasVideoPackage ? "Video upload enabled" : "Purchase video package to enable"
                });
            }

            return new JsonResult(new { allowed = false, message = "Video upload not available" });
        }
    }
}
