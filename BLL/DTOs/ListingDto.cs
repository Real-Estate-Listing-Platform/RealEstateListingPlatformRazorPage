using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class ListingDto
    {
        public Guid Id { get; set; }
        public Guid ListerId { get; set; }
        public string Title { get; set; } = null!;
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
        public string? Status { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Package and payment tracking
        public Guid? UserPackageId { get; set; }
        public bool IsFreeListingorder { get; set; }
        public int MaxPhotos { get; set; }
        public bool AllowVideo { get; set; }
        public bool IsBoosted { get; set; }
        
        // Navigation properties as DTOs
        public string? ListerName { get; set; }
        public string? ListerEmail { get; set; }
        public List<ListingMediaDto> ListingMedia { get; set; } = new();
        
        // Tracking fields
        public Guid? PendingSnapshotId { get; set; }
    }
}
