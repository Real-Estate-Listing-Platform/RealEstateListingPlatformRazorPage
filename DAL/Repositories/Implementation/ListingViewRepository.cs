using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class ListingViewRepository : IListingViewRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingViewRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task AddViewAsync(ListingView view)
        {
            await _context.ListingViews.AddAsync(view);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalViewsAsync(Guid listingId)
        {
            return await _context.ListingViews
                .Where(v => v.ListingId == listingId)
                .CountAsync();
        }

        public async Task<int> GetViewCountByDateRangeAsync(Guid listingId, DateTime startDate, DateTime endDate)
        {
            return await _context.ListingViews
                .Where(v => v.ListingId == listingId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
                .CountAsync();
        }

        public async Task<List<ViewStatistic>> GetDailyViewStatisticsAsync(Guid listingId, DateTime startDate, DateTime endDate)
        {
            var views = await _context.ListingViews
                .Where(v => v.ListingId == listingId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new ViewStatistic
                {
                    Date = g.Key,
                    ViewCount = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            // Fill in missing dates with 0 views
            var result = new List<ViewStatistic>();
            var currentDate = startDate.Date;
            
            while (currentDate <= endDate.Date)
            {
                var stat = views.FirstOrDefault(v => v.Date == currentDate);
                result.Add(new ViewStatistic
                {
                    Date = currentDate,
                    ViewCount = stat?.ViewCount ?? 0
                });
                
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<List<ListingView>> GetRecentViewsAsync(Guid listingId, int count = 10)
        {
            return await _context.ListingViews
                .Where(v => v.ListingId == listingId)
                .Include(v => v.User)
                .OrderByDescending(v => v.ViewedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> HasRecentViewAsync(Guid listingId, Guid? userId, string? ipAddress, int minutesThreshold = 30)
        {
            var thresholdTime = DateTime.UtcNow.AddMinutes(-minutesThreshold);

            if (userId.HasValue)
            {
                // Check by user ID first
                return await _context.ListingViews
                    .AnyAsync(v => v.ListingId == listingId && 
                                   v.UserId == userId && 
                                   v.ViewedAt >= thresholdTime);
            }
            else if (!string.IsNullOrEmpty(ipAddress))
            {
                // Check by IP address for anonymous users
                return await _context.ListingViews
                    .AnyAsync(v => v.ListingId == listingId && 
                                   v.IpAddress == ipAddress && 
                                   v.ViewedAt >= thresholdTime);
            }

            return false;
        }

        // Statistics Methods for Admin Dashboard
        public async Task<int> GetTotalViewsAsync()
        {
            return await _context.ListingViews.CountAsync();
        }

        public async Task<int> GetViewsCountAsync(DateTime startDate)
        {
            return await _context.ListingViews
                .Where(v => v.ViewedAt >= startDate)
                .CountAsync();
        }

        public async Task<List<(Guid ListingId, string Title, int ViewCount, decimal Price, string ImageUrl)>> GetMostViewedListingsAsync(int topCount)
        {
            var mostViewed = await _context.ListingViews
                .Include(v => v.Listing)
                    .ThenInclude(l => l.ListingMedia)
                .Where(v => v.Listing.Status == "Published")
                .GroupBy(v => v.ListingId)
                .Select(g => new
                {
                    ListingId = g.Key,
                    ViewCount = g.Count(),
                    Listing = g.FirstOrDefault()!.Listing
                })
                .OrderByDescending(x => x.ViewCount)
                .Take(topCount)
                .ToListAsync();

            return mostViewed.Select(m => (
                m.ListingId,
                m.Listing.Title,
                m.ViewCount,
                m.Listing.Price,
                m.Listing.ListingMedia.OrderBy(lm => lm.SortOrder).FirstOrDefault()?.Url ?? string.Empty
            )).ToList();
        }

        public async Task<List<(int Hour, int Count)>> GetPeakActivityHoursAsync()
        {
            var peakHours = await _context.ListingViews
                .GroupBy(v => v.ViewedAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return peakHours.Select(p => (p.Hour, p.Count)).ToList();
        }

        public async Task<List<(DateTime Date, int Count)>> GetViewsTrendAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var views = await _context.ListingViews
                .Where(v => v.ViewedAt >= startDate)
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return views.Select(v => (v.Date, v.Count)).ToList();
        }
    }
}

