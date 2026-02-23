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
    public class DashboardModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly ILeadService _leadService;

        public DashboardModel(IListingService listingService, ILeadService leadService)
        {
            _listingService = listingService;
            _leadService = leadService;
        }

        public DashboardStatsDto Stats { get; set; } = new DashboardStatsDto();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            
            // Fetch comprehensive dashboard statistics
            var statsResult = await _leadService.GetDashboardStatsAsync(userId);
            
            if (!statsResult.Success || statsResult.Data == null)
            {
                // Fallback to basic stats if service fails
                var result = await _listingService.GetMyListingsAsync(userId);
                var listings = result.Data ?? new List<ListingDto>();

                Stats = new DashboardStatsDto
                {
                    TotalListings = listings.Count,
                    ActiveListings = listings.Count(l => l.Status == "Published"),
                    PendingReview = listings.Count(l => l.Status == "PendingReview"),
                    DraftListings = listings.Count(l => l.Status == "Draft"),
                    ExpiredListings = listings.Count(l => l.Status == "Expired"),
                    RejectedListings = listings.Count(l => l.Status == "Rejected"),
                    TotalLeads = 0,
                    NewLeads = 0,
                    ContactedLeads = 0,
                    ClosedLeads = 0,
                    ConversionRate = 0.0,
                    TotalViews = 0,
                    AverageViewsPerListing = 0.0,
                    BoostedListings = listings.Count(l => l.IsBoosted),
                    ExpiringListingsSoon = 0,
                    PublishSuccessRate = 0.0,
                    LastLeadReceivedAt = null
                };
            }
            else
            {
                Stats = statsResult.Data;
            }

            return Page();
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
