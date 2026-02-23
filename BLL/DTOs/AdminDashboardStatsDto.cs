namespace BLL.DTOs
{
    public class AdminDashboardStatsDto
    {
        // Listing Statistics
        public int TotalListings { get; set; }
        public int PendingListingsCount { get; set; }
        public int PublishedListingsCount { get; set; }
        public int RejectedListingsCount { get; set; }
        public int DraftListingsCount { get; set; }
        public int ExpiredListingsCount { get; set; }
        public decimal AverageListingPrice { get; set; }
        public int BoostedListingsCount { get; set; }

        // User Statistics
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveListers { get; set; }
        public int ActiveSeekers { get; set; }
        public int UsersRegisteredLast30Days { get; set; }
        public int VerifiedUsers { get; set; }
        public int UnverifiedUsers { get; set; }

        // Lead/Engagement Metrics
        public int TotalLeads { get; set; }
        public int NewLeadsToday { get; set; }
        public int NewLeadsThisWeek { get; set; }
        public int NewLeadsThisMonth { get; set; }
        public int LeadsNew { get; set; }
        public int LeadsContacted { get; set; }
        public int LeadsClosed { get; set; }
        public decimal LeadConversionRate { get; set; }
        public List<TopPerformingListingDto> TopPerformingListings { get; set; } = new();

        // Activity Metrics
        public int TotalViews { get; set; }
        public int ViewsToday { get; set; }
        public int ViewsThisWeek { get; set; }
        public int ViewsThisMonth { get; set; }
        public List<MostViewedListingDto> MostViewedListings { get; set; } = new();
        public List<PeakActivityHourDto> PeakActivityHours { get; set; } = new();

        // Time-Series Data
        public List<TimeSeriesDataDto> ListingsCreatedOverTime { get; set; } = new();
        public List<TimeSeriesDataDto> UserRegistrationsOverTime { get; set; } = new();
        public List<TimeSeriesDataDto> LeadsGeneratedOverTime { get; set; } = new();
        public List<TimeSeriesDataDto> ViewsTrend { get; set; } = new();

        // Revenue Indicators (if available)
        public int TotalPackagesPurchased { get; set; }
        public int BoostPackagesSold { get; set; }
        public int PhotoPacksSold { get; set; }
        public int AdditionalListingsSold { get; set; }
        public decimal TotalRevenue { get; set; }

        // Reports Statistics
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int ResolvedReports { get; set; }
        public int UrgentReports { get; set; }
    }

    public class MostViewedListingDto
    {
        public Guid ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class PeakActivityHourDto
    {
        public int Hour { get; set; }
        public int ActivityCount { get; set; }
        public string DisplayLabel => $"{Hour}:00";
    }

    public class TimeSeriesDataDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string DateLabel { get; set; } = string.Empty;
    }
}

