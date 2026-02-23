namespace BLL.DTOs;

public class PackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string PackageType { get; set; } = null!;
    public decimal Price { get; set; }
    public int? DurationDays { get; set; }
    public int? ListingCount { get; set; }
    public int? PhotoLimit { get; set; }
    public bool AllowVideo { get; set; }
    public int? BoostDays { get; set; }
    public bool IsActive { get; set; }
}

public class PurchasePackageDto
{
    public Guid PackageId { get; set; }
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
    public string? Notes { get; set; }
}

public class UserPackageDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TransactionId { get; set; }
    public PackageDto Package { get; set; } = null!;
    public int? RemainingListings { get; set; }
    public int? RemainingPhotos { get; set; }
    public bool VideoAvailable { get; set; }
    public int? RemainingBoosts { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = null!;
}

public class ApplyPackageDto
{
    public Guid UserPackageId { get; set; }
    public Guid ListingId { get; set; }
}

public class BoostListingDto
{
    public Guid ListingId { get; set; }
    public Guid? UserPackageId { get; set; } // Optional: use existing package or purchase new boost
    public int BoostDays { get; set; } = 7; // Default 7 days
}
