using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Seeker
{
    [Authorize]
    public class InterestedListingsModel : PageModel
    {
        private readonly ILeadService _leadService;

        public InterestedListingsModel(ILeadService leadService)
        {
            _leadService = leadService;
        }

        public List<LeadViewModel> LeadViewModels { get; set; } = new();
        public string Title { get; set; } = "My Interested Listings";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToPage("/Account/Login");
            }

            var result = await _leadService.GetMyLeadsAsSeekerAsync(userId);

            if (result.Success && result.Data != null)
            {
                LeadViewModels = result.Data.Select(l => new LeadViewModel
                {
                    Id = l.Id,
                    ListingId = l.ListingId,
                    ListingTitle = l.Listing?.Title ?? "N/A",
                    ListingAddress = $"{l.Listing?.StreetName}, {l.Listing?.Ward}, {l.Listing?.District}, {l.Listing?.City}",
                    ListingImageUrl = l.Listing?.ListingMedia?.FirstOrDefault()?.Url ?? "",
                    ListingPrice = l.Listing?.Price ?? 0,
                    SeekerName = l.Seeker?.DisplayName ?? "N/A",
                    SeekerEmail = l.Seeker?.Email ?? "N/A",
                    SeekerPhone = l.Seeker?.Phone,
                    Message = l.Message,
                    Status = l.Status ?? "New",
                    AppointmentDate = l.AppointmentDate,
                    ListerNote = l.ListerNote,
                    ListerName = l.Lister?.DisplayName ?? "N/A",
                    CreatedAt = l.CreatedAt ?? DateTime.UtcNow,
                    // Additional listing details
                    TransactionType = l.Listing?.TransactionType,
                    PropertyType = l.Listing?.PropertyType,
                    Area = l.Listing?.Area,
                    Bedrooms = l.Listing?.Bedrooms,
                    Bathrooms = l.Listing?.Bathrooms
                }).ToList();
            }

            return Page();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }
    }
}
