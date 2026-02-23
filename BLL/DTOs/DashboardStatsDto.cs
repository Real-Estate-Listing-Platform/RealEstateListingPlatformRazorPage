namespace BLL.DTOs
{
    /// <summary>
    /// DTO for Lister Dashboard Statistics
    /// </summary>
    public class DashboardStatsDto
    {
        // === SECTION 1: Listing Metrics ===
        public int TotalListings { get; set; }
        public int ActiveListings { get; set; }  // Published
        public int PendingReview { get; set; }
        public int DraftListings { get; set; }
        public int ExpiredListings { get; set; }
        public int RejectedListings { get; set; }
        public double PublishSuccessRate { get; set; }  // Published / Total %

        // === SECTION 2: Lead/Customer Metrics ===
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int ContactedLeads { get; set; }
        public int ClosedLeads { get; set; }
        public double ConversionRate { get; set; }  // Closed / Total %

        // === SECTION 3: View/Engagement Metrics (Last 30 Days) ===
        public int TotalViews { get; set; }
        public double AverageViewsPerListing { get; set; }
        public string? MostViewedListingTitle { get; set; }
        public int MostViewedListingViews { get; set; }
        public Guid? MostViewedListingId { get; set; }

        // === SECTION 4: Package/Subscription Metrics ===
        public int ActivePackages { get; set; }
        public int BoostedListings { get; set; }
        public int RemainingPhotoCapacity { get; set; }
        public int? DaysUntilNextExpiration { get; set; }
        public DateTime? NextPackageExpiration { get; set; }

        // === Additional Helper Properties ===
        public int ExpiringListingsSoon { get; set; }  // Expiring in next 7 days
        public DateTime? LastLeadReceivedAt { get; set; }

        // === Chart Data ===
        public List<ChartDataPoint> ViewsChartData { get; set; } = new();
        public List<ChartDataPoint> LeadsChartData { get; set; } = new();
        public List<ChartDataPoint> ConversionChartData { get; set; } = new();

        // === Top Performing Listings ===
        public List<TopPerformingListingDto> TopPerformingListings { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;  // Date label
        public int Value { get; set; }  // Metric value
    }

    public class TopPerformingListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LeadCount { get; set; }
        public decimal EngagementScore { get; set; }  // Combined score
    }
}
