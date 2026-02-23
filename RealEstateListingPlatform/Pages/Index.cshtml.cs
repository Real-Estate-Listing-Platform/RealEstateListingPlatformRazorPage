using System.Diagnostics;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IListingService _listingService;

        public IndexModel(ILogger<IndexModel> logger, IListingService listingService)
        {
            _logger = logger;
            _listingService = listingService;
        }

        public List<ListingApprovalViewModel> Properties { get; set; } = new();

        public async Task OnGetAsync()
        {
            var listings = await _listingService.GetPublishedListingsAsync();
            
            // Take first 6 listings (boosted ones will be first due to repository ordering)
            Properties = listings.Take(6).Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Address = $"{l.District}, {l.City}",
                Price = l.Price,
                Bedrooms = l.Bedrooms ?? 0,
                Bathrooms = l.Bathrooms ?? 0,
                Floors = l.Floors,
                Area = l.Area ?? "0",
                TransactionType = l.TransactionType == "Sell" ? "For Sale" : "For Rent",
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                   ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp"
            }).ToList();
        }
    }
}
