using System.ComponentModel.DataAnnotations;

namespace DAL.Models;

public class UserPackage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid PackageId { get; set; }

    [Required]
    public Guid TransactionId { get; set; }

    public int? RemainingListings { get; set; } // Null for unlimited/not applicable

    public int? RemainingPhotos { get; set; } // Additional photos remaining

    public bool VideoAvailable { get; set; } = false;

    public int? RemainingBoosts { get; set; } // Boost credits remaining

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(20)]
    public string Status { get; set; } = "Active"; // "Active", "Expired", "Used", "Cancelled"

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ListingPackage Package { get; set; } = null!;
    public virtual Transaction Transaction { get; set; } = null!;
}
