using System;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public partial class ListingSnapshot
{
    public Guid Id { get; set; }
    
    public Guid ListingId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Original listing data (snapshot before edit)
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
    
    // Serialized media URLs (JSON format)
    public string? MediaUrlsJson { get; set; }
    
    // Navigation property
    public virtual Listing Listing { get; set; } = null!;
}
