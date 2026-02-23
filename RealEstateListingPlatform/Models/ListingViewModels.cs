using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BLL.DTOs;

namespace RealEstateListingPlatform.Models
{
    public class ListingCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Property type is required")]
        public string PropertyType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string? StreetName { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? HouseNumber { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [Range(0, 100)]
        public int? Bedrooms { get; set; }

        [Range(0, 100)]
        public int? Bathrooms { get; set; }

        [Range(0, 100)]
        public int? Floors { get; set; }

        public string? LegalStatus { get; set; }
        public string? FurnitureStatus { get; set; }
        public string? Direction { get; set; }
    }

    public class ListingEditViewModel : ListingCreateViewModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ListingMediaDto>? ExistingMedia { get; set; }
        
        // Package-related properties
        public bool IsBoosted { get; set; }
        public bool IsFreeListingorder { get; set; }
        public int MaxPhotos { get; set; } = 5;
        public bool AllowVideo { get; set; }
    }
}
