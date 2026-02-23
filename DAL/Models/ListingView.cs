using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class ListingView
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ListingId { get; set; }

        public Guid? UserId { get; set; } // Nullable for anonymous views

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Listing Listing { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
