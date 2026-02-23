using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public class ListingPackage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string PackageType { get; set; } = null!; // "FREE", "ADDITIONAL_LISTING", "PHOTO_PACK", "VIDEO_UPLOAD", "BOOST_LISTING"

    [Precision(18, 2)]
    public decimal Price { get; set; }

    public int? DurationDays { get; set; } = 30; // Default 30 days for listings

    public int? ListingCount { get; set; } // Number of listings included (1 for additional listing)

    public int? PhotoLimit { get; set; } // Max photos (5 for free, +10 for photo pack)

    public bool AllowVideo { get; set; } = false;

    public int? BoostDays { get; set; } // Days to keep listing at top

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}
