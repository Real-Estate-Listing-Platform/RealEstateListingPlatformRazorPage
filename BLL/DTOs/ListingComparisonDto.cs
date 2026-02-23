using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class ListingComparisonDto
    {
        public Guid ListingId { get; set; }
        public string ListerName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsUpdate { get; set; } // true if this is an edit, false if new listing
        
        // Original data (before edit)
        public ListingDataDto? Original { get; set; }
        
        // Current/Modified data (after edit)
        public ListingDataDto Current { get; set; } = new();
    }

    public class ListingDataDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TransactionType { get; set; }
        public string? PropertyType { get; set; }
        public decimal Price { get; set; }
        public string? StreetName { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? HouseNumber { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Floors { get; set; }
        public string? LegalStatus { get; set; }
        public string? FurnitureStatus { get; set; }
        public string? Direction { get; set; }
        public List<string> MediaUrls { get; set; } = new();
        
        public string FormattedAddress => $"{HouseNumber}, {StreetName}, {Ward}, {District}, {City}";
        public string FormattedPrice
        {
            get
            {
                if (Price >= 1000000000)
                    return $"{Price / 1000000000:N1} tỉ";
                if (Price >= 1000000)
                    return $"{Price / 1000000:N0} triệu";
                return Price.ToString("N0") + " VND";
            }
        }
    }
}
