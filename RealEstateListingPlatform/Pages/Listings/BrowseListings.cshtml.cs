using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Pages.Listings
{
    public class BrowseListingsModel : PageModel
    {
        private readonly IListingService _listingService;

        public BrowseListingsModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        public List<ListingApprovalViewModel> Listings { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } = string.Empty;
        
        [BindProperty(SupportsGet = true)]
        public List<string>? PropertyType { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? MaxPrice { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public new int Page { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;

        public string Title { get; set; } = string.Empty;
        public List<string> FilterPropertyTypes { get; set; } = new();
        public string? FilterLocation { get; set; }
        public decimal? FilterMaxPrice { get; set; }
        public string? FilterMaxPriceRaw { get; set; }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            string dbType = Type.Equals("Sell", StringComparison.OrdinalIgnoreCase) ? "Sell" : "Rent";
            var listings = await _listingService.GetPublishedByTypeAsync(dbType);
            decimal? maxPriceNum = TryParseMaxPrice(MaxPrice);
            int totalCount = 0;

            if (listings != null)
            {
                if (PropertyType != null && PropertyType.Count > 0)
                {
                    var types = PropertyType.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
                    if (types.Count > 0)
                        listings = listings.Where(l => types.Any(t => string.Equals(t, l.PropertyType, StringComparison.OrdinalIgnoreCase))).ToList();
                }
                if (!string.IsNullOrWhiteSpace(Location))
                {
                    var loc = Location.Trim();
                    listings = listings.Where(l =>
                        (l.StreetName != null && l.StreetName.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.Ward != null && l.Ward.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.District != null && l.District.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.City != null && l.City.Contains(loc, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                if (maxPriceNum.HasValue && maxPriceNum.Value > 0)
                    listings = listings.Where(l => l.Price <= maxPriceNum.Value).ToList();
                
                // Re-apply boost ordering after filtering (boosted listings first)
                listings = listings.OrderByDescending(l => l.IsBoosted)
                                 .ThenByDescending(l => l.CreatedAt)
                                 .ToList();
            }

            // Calculate pagination
            totalCount = listings?.Count() ?? 0;
            var paginatedListings = listings?.Skip((Page - 1) * PageSize).Take(PageSize).ToList() ?? new List<ListingDto>();

            Listings = paginatedListings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description ?? "N/A",
                Price = l.Price,
                PropertyType = l.PropertyType ?? "N/A",
                TransactionType = l.TransactionType ?? dbType,
                ListerName = l.ListerName ?? "Unknown User",
                Area = l.Area ?? "0",
                Address = $"{l.HouseNumber}, {l.StreetName}, {l.Ward}, {l.District}, {l.City}",
                Bedrooms = l.Bedrooms,
                Bathrooms = l.Bathrooms,
                Floors = l.Floors,
                LegalStatus = l.LegalStatus ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                           ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp",
                IsBoosted = l.IsBoosted
            }).ToList();

            Title = Type.Equals("Sell", StringComparison.OrdinalIgnoreCase) ? "Bất động sản cho bán" : "Bất động sản cho thuê";
            FilterPropertyTypes = PropertyType ?? new List<string>();
            FilterLocation = Location;
            FilterMaxPrice = maxPriceNum;
            FilterMaxPriceRaw = MaxPrice;
            CurrentPage = Page;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        }

        private static decimal? TryParseMaxPrice(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var t = s.Trim();
            if (decimal.TryParse(t.Replace(",", "").Replace(" ", ""), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v < 1_000_000 ? v * 1_000_000_000 : v;
            var m = System.Text.RegularExpressions.Regex.Match(t, @"(\d+(?:[.,]\d+)?)\s*(tỷ|ty|B|b)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(",", "."), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var num))
                return string.IsNullOrEmpty(m.Groups[2].Value) ? (num < 1_000_000 ? num * 1_000_000_000 : num) : num * 1_000_000_000;
            return null;
        }
    }
}
