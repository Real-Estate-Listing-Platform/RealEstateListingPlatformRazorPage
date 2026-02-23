using System;
using System.Collections.Generic;

namespace RealEstateListingPlatform.Models
{
    public class ListingComparisonViewModel
    {
        public Guid ListingId { get; set; }
        public string ListerName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsUpdate { get; set; } // true if this is an edit, false if new listing
        
        // Original data (before edit)
        public ListingDataViewModel? Original { get; set; }
        
        // Current/Modified data (after edit)
        public ListingDataViewModel Current { get; set; } = new();

        /// <summary>
        /// Checks if a specific field has changed between Original and Current versions
        /// </summary>
        public bool HasChanged(string fieldName)
        {
            if (!IsUpdate || Original == null)
                return false;

            return fieldName switch
            {
                "Title" => Original.Title != Current.Title,
                "Description" => Original.Description != Current.Description,
                "TransactionType" => Original.TransactionType != Current.TransactionType,
                "PropertyType" => Original.PropertyType != Current.PropertyType,
                "Price" => Original.Price != Current.Price,
                "Address" => Original.FormattedAddress != Current.FormattedAddress,
                "StreetName" => Original.StreetName != Current.StreetName,
                "Ward" => Original.Ward != Current.Ward,
                "District" => Original.District != Current.District,
                "City" => Original.City != Current.City,
                "Area" => Original.Area != Current.Area,
                "HouseNumber" => Original.HouseNumber != Current.HouseNumber,
                "Bedrooms" => Original.Bedrooms != Current.Bedrooms,
                "Bathrooms" => Original.Bathrooms != Current.Bathrooms,
                "Floors" => Original.Floors != Current.Floors,
                "LegalStatus" => Original.LegalStatus != Current.LegalStatus,
                "FurnitureStatus" => Original.FurnitureStatus != Current.FurnitureStatus,
                "Direction" => Original.Direction != Current.Direction,
                "Media" => !Original.MediaUrls.SequenceEqual(Current.MediaUrls),
                _ => false
            };
        }
    }

    public class ListingDataViewModel
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
                    return $"{Price / 1000000000:N1} t?";
                if (Price >= 1000000)
                    return $"{Price / 1000000:N0} tri?u";
                return Price.ToString("N0") + " VND";
            }
        }
    }
}
