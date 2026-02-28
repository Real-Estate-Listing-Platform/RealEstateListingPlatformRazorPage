using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IListingService _listingService;

        public DetailsModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        public ListingDto Listing { get; set; } = new ListingDto();
        public ListingViewStats? ViewStats { get; set; }

        // Proxy properties to expose Listing properties directly for view access
        public Guid Id => Listing?.Id ?? Guid.Empty;
        public string Title => Listing?.Title ?? "";
        public string? Description => Listing?.Description;
        public decimal Price => Listing?.Price ?? 0;
        public string? TransactionType => Listing?.TransactionType;
        public string? PropertyType => Listing?.PropertyType;
        public string? StreetName => Listing?.StreetName;
        public string? Ward => Listing?.Ward;
        public string? District => Listing?.District;
        public string? City => Listing?.City;
        public string? HouseNumber => Listing?.HouseNumber;
        public string? Area => Listing?.Area;
        public decimal? Latitude => Listing?.Latitude;
        public decimal? Longitude => Listing?.Longitude;
        public int? Bedrooms => Listing?.Bedrooms;
        public int? Bathrooms => Listing?.Bathrooms;
        public int? Floors => Listing?.Floors;
        public string? LegalStatus => Listing?.LegalStatus;
        public string? FurnitureStatus => Listing?.FurnitureStatus;
        public string? Direction => Listing?.Direction;
        public string? Status => Listing?.Status;
        public DateTime? ExpirationDate => Listing?.ExpirationDate;
        public bool IsBoosted => Listing?.IsBoosted ?? false;
        public bool IsFreeListingorder => Listing?.IsFreeListingorder ?? false;
        public int MaxPhotos => Listing?.MaxPhotos ?? 0;
        public bool AllowVideo => Listing?.AllowVideo ?? false;
        public List<ListingMediaDto> ListingMedia => Listing?.ListingMedia ?? new List<ListingMediaDto>();
        public DateTime? CreatedAt => Listing?.CreatedAt;
        public DateTime? UpdatedAt => Listing?.UpdatedAt;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            
            // Verify ownership
            if (!await _listingService.CanUserModifyListingAsync(id, userId))
            {
                TempData["Error"] = "You are not authorized to view this listing.";
                return RedirectToPage("/Lister/Listings");
            }

            var result = await _listingService.GetListingWithMediaAsync(id);
            
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Listing not found.";
                return RedirectToPage("/Lister/Listings");
            }

            Listing = result.Data;

            // Get view statistics
            var viewStatsResult = await _listingService.GetListingViewStatsAsync(id, 30);
            if (viewStatsResult.Success && viewStatsResult.Data != null)
            {
                ViewStats = viewStatsResult.Data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitForReviewAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.SubmitForReviewAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToPage("/Lister/Edit", new { id });
            }

            TempData["Success"] = "Listing submitted for review.";
            return RedirectToPage("/Lister/Listings");
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

            return RedirectToPage("/Lister/Listings");
        }

        public async Task<IActionResult> OnGetViewStatsAsync(Guid id)
        {
            var userId = GetCurrentUserId();

            if (!await _listingService.CanUserModifyListingAsync(id, userId))
            {
                return new JsonResult(new { success = false });
            }

            var result = await _listingService.GetListingViewStatsAsync(id, 30);
            if (!result.Success || result.Data == null)
            {
                return new JsonResult(new { success = false });
            }

            var stats = result.Data;
            return new JsonResult(new
            {
                success = true,
                data = new
                {
                    totalViews = stats.TotalViews,
                    viewsToday = stats.ViewsToday,
                    viewsThisWeek = stats.ViewsThisWeek,
                    viewsThisMonth = stats.ViewsThisMonth,
                    dailyStats = stats.DailyStats.Select(s => new { date = s.Date, views = s.Views })
                }
            });
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
