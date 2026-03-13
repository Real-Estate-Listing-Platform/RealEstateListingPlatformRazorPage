using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class ValuationReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        [StringLength(200)]
        public string? ReportName { get; set; }

        [Required]
        [StringLength(50)]
        public string PropertyType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty;

        [Precision(10, 2)]
        public decimal AreaSqm { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Ward { get; set; }

        [StringLength(250)]
        public string? AddressLine { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Estimation results
        [Precision(18, 2)]
        public decimal? EstimatedMinPrice { get; set; }

        [Precision(18, 2)]
        public decimal? EstimatedAvgPrice { get; set; }

        [Precision(18, 2)]
        public decimal? EstimatedMaxPrice { get; set; }

        [Precision(18, 2)]
        public decimal? AvgPricePerSqm { get; set; }

        public int SampleCount { get; set; }

        public bool IsFallbackToCity { get; set; }

        [StringLength(1000)]
        public string MarketInsight { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
