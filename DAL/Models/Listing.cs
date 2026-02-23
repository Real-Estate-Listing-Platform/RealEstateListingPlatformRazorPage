using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public partial class Listing
{
    public Guid Id { get; set; }

    public Guid ListerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? TransactionType { get; set; }

    public string? PropertyType { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }

    public string? StreetName { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public string? Area { get; set; }

    public string? HouseNumber { get; set; }

    [Precision(9, 6)]
    public decimal? Latitude { get; set; }

    [Precision(9, 6)]
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
    public Guid? UserPackageId { get; set; } // Reference to package used for this listing
    public bool IsFreeListingorder { get; set; } = true; // True if using free tier
    public int MaxPhotos { get; set; } = 5; // Default 5 photos for free tier
    public bool AllowVideo { get; set; } = false; // Video upload permission
    public bool IsBoosted { get; set; } = false; // Is currently boosted to top

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual User Lister { get; set; } = null!;

    public virtual ICollection<ListingMedia> ListingMedia { get; set; } = new List<ListingMedia>();

    public virtual ICollection<ListingPriceHistory> ListingPriceHistories { get; set; } = new List<ListingPriceHistory>();

    public virtual ListingTour360? ListingTour360 { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual UserPackage? UserPackage { get; set; }

    public virtual ICollection<ListingBoost> ListingBoosts { get; set; } = new List<ListingBoost>();

    public virtual ICollection<ListingView> ListingViews { get; set; } = new List<ListingView>();

    public virtual ICollection<ListingSnapshot> ListingSnapshots { get; set; } = new List<ListingSnapshot>();
    
    // Tracking field for pending edit approval
    public Guid? PendingSnapshotId { get; set; }
    public virtual ListingSnapshot? PendingSnapshot { get; set; }
}
