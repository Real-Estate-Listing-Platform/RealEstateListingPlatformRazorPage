using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class MyPackagesModel : PageModel
    {
        private readonly IPackageService _packageService;

        public MyPackagesModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PackageType { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? PurchasedAfter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? PurchasedBefore { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "PurchasedAt";

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public PaginatedResult<UserPackageDto> Packages { get; set; } = new();
        public List<UserPackageDto> ActivePackages { get; set; } = new();

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();

            var filterParams = new PackageFilterParameters
            {
                SearchTerm = SearchTerm,
                Status = Status,
                PackageType = PackageType,
                PurchasedAfter = PurchasedAfter,
                PurchasedBefore = PurchasedBefore,
                SortBy = SortBy,
                SortOrder = SortOrder,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _packageService.GetUserPackagesFilteredAsync(userId, filterParams);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                Packages = new PaginatedResult<UserPackageDto>();
                return Page();
            }

            Packages = result.Data ?? new PaginatedResult<UserPackageDto>();

            var activeResult = await _packageService.GetActiveUserPackagesAsync(userId);
            ActivePackages = activeResult.Data ?? new List<UserPackageDto>();

            return Page();
        }
    }
}
