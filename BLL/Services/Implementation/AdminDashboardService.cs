using BLL.DTOs;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementation
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILeadRepository _leadRepository;
        private readonly IListingViewRepository _listingViewRepository;
        private readonly IReportRepository? _reportRepository;
        private readonly IPackageRepository? _packageRepository;

        public AdminDashboardService(
            IListingRepository listingRepository,
            IUserRepository userRepository,
            ILeadRepository leadRepository,
            IListingViewRepository listingViewRepository,
            IReportRepository? reportRepository = null,
            IPackageRepository? packageRepository = null)
        {
            _listingRepository = listingRepository;
            _userRepository = userRepository;
            _leadRepository = leadRepository;
            _listingViewRepository = listingViewRepository;
            _reportRepository = reportRepository;
            _packageRepository = packageRepository;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStatsDto();

            // Calculate date ranges
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.AddDays(-7);
            var monthStart = now.AddDays(-30);

            // Listing Statistics
            stats.TotalListings = await _listingRepository.GetTotalListingsCountAsync();
            stats.PendingListingsCount = await _listingRepository.GetListingsCountByStatusAsync("PendingReview");
            stats.PublishedListingsCount = await _listingRepository.GetListingsCountByStatusAsync("Published");
            stats.RejectedListingsCount = await _listingRepository.GetListingsCountByStatusAsync("Rejected");
            stats.DraftListingsCount = await _listingRepository.GetListingsCountByStatusAsync("Draft");
            stats.ExpiredListingsCount = await _listingRepository.GetListingsCountByStatusAsync("Expired");
            stats.AverageListingPrice = await _listingRepository.GetAverageListingPriceAsync();
            stats.BoostedListingsCount = await _listingRepository.GetBoostedListingsCountAsync();

            // User Statistics
            stats.TotalUsers = await _userRepository.GetTotalUsersCountAsync();
            stats.NewUsersToday = await _userRepository.GetNewUsersCountAsync(todayStart);
            stats.NewUsersThisWeek = await _userRepository.GetNewUsersCountAsync(weekStart);
            stats.NewUsersThisMonth = await _userRepository.GetNewUsersCountAsync(monthStart);
            stats.ActiveListers = await _userRepository.GetActiveListersCountAsync();
            stats.ActiveSeekers = await _userRepository.GetActiveSeekersCountAsync();
            stats.UsersRegisteredLast30Days = stats.NewUsersThisMonth;
            stats.VerifiedUsers = await _userRepository.GetVerifiedUsersCountAsync();
            stats.UnverifiedUsers = await _userRepository.GetUnverifiedUsersCountAsync();

            // Lead/Engagement Metrics
            stats.TotalLeads = await _leadRepository.GetTotalLeadsCountAsync();
            stats.NewLeadsToday = await _leadRepository.GetNewLeadsCountAsync(todayStart);
            stats.NewLeadsThisWeek = await _leadRepository.GetNewLeadsCountAsync(weekStart);
            stats.NewLeadsThisMonth = await _leadRepository.GetNewLeadsCountAsync(monthStart);
            stats.LeadsNew = await _leadRepository.GetLeadsCountByStatusAsync("New");
            stats.LeadsContacted = await _leadRepository.GetLeadsCountByStatusAsync("Contacted");
            stats.LeadsClosed = await _leadRepository.GetLeadsCountByStatusAsync("Closed");
            
            // Calculate lead conversion rate
            if (stats.TotalLeads > 0)
            {
                stats.LeadConversionRate = (decimal)stats.LeadsClosed / stats.TotalLeads * 100;
            }

            // Top Performing Listings
            var topListings = await _listingRepository.GetTopPerformingListingsAsync(5);
            stats.TopPerformingListings = topListings.Select(t => new TopPerformingListingDto
            {
                Id = t.ListingId,
                Title = t.Title,
                LeadCount = t.LeadCount,
                ViewCount = 0, // Not available in this context
                EngagementScore = t.LeadCount * 10 // Simple score calculation
            }).ToList();

            // Activity Metrics (Views)
            stats.TotalViews = await _listingViewRepository.GetTotalViewsAsync();
            stats.ViewsToday = await _listingViewRepository.GetViewsCountAsync(todayStart);
            stats.ViewsThisWeek = await _listingViewRepository.GetViewsCountAsync(weekStart);
            stats.ViewsThisMonth = await _listingViewRepository.GetViewsCountAsync(monthStart);

            // Most Viewed Listings
            var mostViewed = await _listingViewRepository.GetMostViewedListingsAsync(5);
            stats.MostViewedListings = mostViewed.Select(m => new MostViewedListingDto
            {
                ListingId = m.ListingId,
                Title = m.Title,
                ViewCount = m.ViewCount,
                Price = m.Price,
                ImageUrl = m.ImageUrl
            }).ToList();

            // Peak Activity Hours
            var peakHours = await _listingViewRepository.GetPeakActivityHoursAsync();
            stats.PeakActivityHours = peakHours.Select(p => new PeakActivityHourDto
            {
                Hour = p.Hour,
                ActivityCount = p.Count
            }).ToList();

            // Time-Series Data (Last 30 days)
            var listingsOverTime = await _listingRepository.GetListingsCreatedOverTimeAsync(30);
            stats.ListingsCreatedOverTime = listingsOverTime.Select(l => new TimeSeriesDataDto
            {
                Date = l.Date,
                Count = l.Count,
                DateLabel = l.Date.ToString("MMM dd")
            }).ToList();

            var usersOverTime = await _userRepository.GetUserRegistrationsOverTimeAsync(30);
            stats.UserRegistrationsOverTime = usersOverTime.Select(u => new TimeSeriesDataDto
            {
                Date = u.Date,
                Count = u.Count,
                DateLabel = u.Date.ToString("MMM dd")
            }).ToList();

            var leadsOverTime = await _leadRepository.GetLeadsGeneratedOverTimeAsync(30);
            stats.LeadsGeneratedOverTime = leadsOverTime.Select(l => new TimeSeriesDataDto
            {
                Date = l.Date,
                Count = l.Count,
                DateLabel = l.Date.ToString("MMM dd")
            }).ToList();

            var viewsOverTime = await _listingViewRepository.GetViewsTrendAsync(30);
            stats.ViewsTrend = viewsOverTime.Select(v => new TimeSeriesDataDto
            {
                Date = v.Date,
                Count = v.Count,
                DateLabel = v.Date.ToString("MMM dd")
            }).ToList();

            // Revenue Indicators (if package repository exists)
            if (_packageRepository != null)
            {
                try
                {
                    var packageStats = await _packageRepository.GetPackageStatisticsAsync();
                    stats.TotalPackagesPurchased = packageStats.TotalPurchased;
                    stats.BoostPackagesSold = packageStats.BoostPackages;
                    stats.PhotoPacksSold = packageStats.PhotoPacks;
                    stats.AdditionalListingsSold = packageStats.AdditionalListings;
                    stats.TotalRevenue = packageStats.TotalRevenue;
                }
                catch
                {
                    // If package stats not available, set to 0
                    stats.TotalPackagesPurchased = 0;
                    stats.BoostPackagesSold = 0;
                    stats.PhotoPacksSold = 0;
                    stats.AdditionalListingsSold = 0;
                    stats.TotalRevenue = 0;
                }
            }

            // Reports Statistics
            if (_reportRepository != null)
            {
                try
                {
                    stats.TotalReports = await _reportRepository.GetTotalReportsCountAsync();
                    stats.PendingReports = await _reportRepository.GetReportsCountByStatusAsync("Pending");
                    stats.ResolvedReports = await _reportRepository.GetReportsCountByStatusAsync("Resolved");
                    stats.UrgentReports = await _reportRepository.GetUrgentReportsCountAsync();
                }
                catch
                {
                    // If reports not available, set to 0
                    stats.TotalReports = 0;
                    stats.PendingReports = 0;
                    stats.ResolvedReports = 0;
                    stats.UrgentReports = 0;
                }
            }

            return stats;
        }
    }
}
