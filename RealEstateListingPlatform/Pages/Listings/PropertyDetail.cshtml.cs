using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using RealEstateListingPlatform.Models;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Listings
{
    public class PropertyDetailModel : PageModel
    {
        private readonly IListingService _listingService;

        public PropertyDetailModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public ListingApprovalViewModel Property { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var property = await _listingService.GetByIdAsync(Id);

            if (property == null)
            {
                return NotFound();
            }

            // Track the view (properly await to prevent DbContext disposal issues)
            try
            {
                var userId = User.Identity?.IsAuthenticated == true
                    ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString())
                    : (Guid?)null;
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                await _listingService.TrackViewAsync(Id, userId, ipAddress, userAgent);
            }
            catch (Exception)
            {
                // Silently ignore view tracking errors - don't break the page
            }

            var mediaUrls = property.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url ?? string.Empty).ToList() ?? new List<string>();
            var defaultImg = "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp";
            if (mediaUrls.Count == 0) mediaUrls.Add(defaultImg);

            Property = new ListingApprovalViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description ?? "N/A",
                Price = property.Price,
                PropertyType = property.PropertyType ?? "N/A",
                TransactionType = property.TransactionType ?? "N/A",
                ListerName = property.ListerName ?? "Unknown User",
                Area = property.Area ?? "0",
                Address = $"{property.HouseNumber}, {property.StreetName}, {property.Ward}, {property.District}, {property.City}",
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Floors = property.Floors,
                LegalStatus = property.LegalStatus ?? "N/A",
                FurnitureStatus = property.FurnitureStatus ?? "N/A",
                Direction = property.Direction ?? "N/A",
                CreatedAt = property.CreatedAt ?? DateTime.Now,
                ImageUrl = mediaUrls.First(),
                ImageUrls = mediaUrls
            };

            return Page();
        }
    }
}
