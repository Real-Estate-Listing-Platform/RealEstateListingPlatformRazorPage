using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ListingsModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPackageService _packageService;

        public ListingsModel(IListingService listingService, IPackageService packageService)
        {
            _listingService = listingService;
            _packageService = packageService;
        }

        public PaginatedResult<ListingDto> Listings { get; set; } = new PaginatedResult<ListingDto>();
        public List<UserPackageDto> BoostPackages { get; set; } = new List<UserPackageDto>();

        // Proxy properties to expose Listings pagination properties directly for view access
        public List<ListingDto> Items => Listings.Items;
        public bool HasPrevious => Listings.HasPrevious;
        public bool HasNext => Listings.HasNext;
        public int TotalCount => Listings.TotalCount;
        public int TotalPages => Listings.TotalPages;
        public int CurrentPage => Listings.PageNumber;
        public int StartIndex => Listings.StartIndex;
        public int EndIndex => Listings.EndIndex;

        // Filter parameters
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TransactionType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PropertyType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? City { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? District { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "CreatedAt";

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        // Messages
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? WarningMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();

            var filterParams = new ListingFilterParameters
            {
                SearchTerm = SearchTerm,
                Status = Status,
                TransactionType = TransactionType,
                PropertyType = PropertyType,
                City = City,
                District = District,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                SortBy = SortBy,
                SortOrder = SortOrder,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var result = await _listingService.GetMyListingsFilteredAsync(userId, filterParams);
            Listings = result.Data ?? new PaginatedResult<ListingDto>();

            // Get active boost packages for the user
            var activePackagesResult = await _packageService.GetActiveUserPackagesAsync(userId);
            BoostPackages = activePackagesResult.Success && activePackagesResult.Data != null
                ? activePackagesResult.Data.Where(p => p.Package.PackageType == "BOOST_LISTING" && p.Status == "Active").ToList()
                : new List<UserPackageDto>();

            // Handle TempData messages
            if (TempData["Success"] != null)
                SuccessMessage = TempData["Success"]?.ToString();
            if (TempData["Error"] != null)
                ErrorMessage = TempData["Error"]?.ToString();
            if (TempData["Warning"] != null)
                WarningMessage = TempData["Warning"]?.ToString();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.DeleteListingAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Listing permanently deleted.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBoostListingAsync(Guid listingId, Guid? userPackageId, int boostDays = 7)
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
            }
            else
            {
                TempData["Success"] = "Listing boosted successfully! Your listing is now at the top.";
            }

            return RedirectToPage();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims");

            return Guid.Parse(userIdClaim);
        }
    }
}
