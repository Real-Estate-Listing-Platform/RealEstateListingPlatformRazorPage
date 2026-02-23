using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public Guid? PackageId { get; set; }

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = null!; // "PACKAGE_PURCHASE", "LISTING_PAYMENT", "BOOST_PAYMENT", "REFUND"

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "VND";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Failed", "Refunded"

    [StringLength(50)]
    public string? PaymentMethod { get; set; } // "MOMO", "VNPAY", "BANK_TRANSFER", "CREDIT_CARD"

    [StringLength(255)]
    public string? PaymentReference { get; set; } // External payment gateway reference

    // PayOS specific fields
    public long? PayOSOrderCode { get; set; } // PayOS order code (unique identifier)
    
    [StringLength(500)]
    public string? PayOSCheckoutUrl { get; set; } // PayOS payment link
    
    public string? PayOSQrCode { get; set; } // PayOS QR code data URL
    
    [StringLength(100)]
    public string? PayOSTransactionId { get; set; } // PayOS transaction reference

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ListingPackage? Package { get; set; }
    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}
