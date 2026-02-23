using System.ComponentModel.DataAnnotations;

namespace DAL.Models;

public class ListingBoost
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ListingId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? UserPackageId { get; set; } // Reference to the package used for boost

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; }

    public int BoostDays { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(50)]
    public string Status { get; set; } = "Active"; // "Active", "Expired", "Cancelled"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Listing Listing { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual UserPackage? UserPackage { get; set; }
}
