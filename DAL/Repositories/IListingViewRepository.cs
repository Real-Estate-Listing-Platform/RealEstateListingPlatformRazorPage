using DAL.Models;

namespace DAL.Repositories
{
    public interface IListingViewRepository
    {
        /// <summary>
        /// Records a view event for a listing
        /// </summary>
        Task AddViewAsync(ListingView view);

        /// <summary>
        /// Gets total view count for a specific listing
        /// </summary>
        Task<int> GetTotalViewsAsync(Guid listingId);

        /// <summary>
        /// Gets view count for a listing within a date range
        /// </summary>
        Task<int> GetViewCountByDateRangeAsync(Guid listingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets daily view statistics for a listing within a date range
        /// </summary>
        Task<List<ViewStatistic>> GetDailyViewStatisticsAsync(Guid listingId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the latest views for a listing
        /// </summary>
        Task<List<ListingView>> GetRecentViewsAsync(Guid listingId, int count = 10);

        /// <summary>
        /// Checks if a user has viewed a listing recently (to prevent duplicate tracking)
        /// </summary>
        Task<bool> HasRecentViewAsync(Guid listingId, Guid? userId, string? ipAddress, int minutesThreshold = 30);

        // Statistics Methods for Admin Dashboard
        Task<int> GetTotalViewsAsync();
        Task<int> GetViewsCountAsync(DateTime startDate);
        Task<List<(Guid ListingId, string Title, int ViewCount, decimal Price, string ImageUrl)>> GetMostViewedListingsAsync(int topCount);
        Task<List<(int Hour, int Count)>> GetPeakActivityHoursAsync();
        Task<List<(DateTime Date, int Count)>> GetViewsTrendAsync(int days);
    }

    /// <summary>
    /// View statistic model for aggregated data
    /// </summary>
    public class ViewStatistic
    {
        public DateTime Date { get; set; }
        public int ViewCount { get; set; }
    }
}
