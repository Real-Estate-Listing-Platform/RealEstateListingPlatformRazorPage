using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;

namespace RealEstateListingPlatform.Pages.Listings
{
    public class IndexModel : PageModel
    {
        private readonly IListingService _listingService;

        public IndexModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        public IEnumerable<ListingDto> Listings { get; set; } = new List<ListingDto>();

        public async Task OnGetAsync()
        {
            Listings = await _listingService.GetListings();
        }
    }
}
