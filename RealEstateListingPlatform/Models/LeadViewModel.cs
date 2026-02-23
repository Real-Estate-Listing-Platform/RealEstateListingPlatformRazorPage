using System.ComponentModel.DataAnnotations;

namespace RealEstateListingPlatform.Models
{
    public class LeadViewModel
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public string ListingAddress { get; set; } = string.Empty;
        public string ListingImageUrl { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string SeekerName { get; set; } = string.Empty;
        public string SeekerEmail { get; set; } = string.Empty;
        public string? SeekerPhone { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = "New";
        public DateTime? AppointmentDate { get; set; }
        public string? ListerNote { get; set; }
        public string? ListerName { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Additional listing details
        public string? TransactionType { get; set; }
        public string? PropertyType { get; set; }
        public string? Area { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    "New" => "badge bg-success",
                    "Contacted" => "badge bg-primary",
                    "Closed" => "badge bg-secondary",
                    _ => "badge bg-light"
                };
            }
        }
    }

    public class CreateLeadDto
    {
        [Required]
        public Guid ListingId { get; set; }

        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string? Message { get; set; }

        public DateTime? AppointmentDate { get; set; }
    }

    public class UpdateLeadStatusDto
    {
        [Required]
        public Guid LeadId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? ListerNote { get; set; }
    }

    public class LeadStatisticsViewModel
    {
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int ContactedLeads { get; set; }
        public int ClosedLeads { get; set; }
        public List<LeadViewModel> RecentLeads { get; set; } = new();
    }
}
