using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class ListingRepository : IListingRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        // Read operations
        public async Task<List<Listing>> GetListings()
        {
            return await _context.Listings.ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetPendingListingsAsync()
        {
            var result = await _context.Listings
                .Include(l => l.Lister).Include(nameof(Listing.ListingMedia))
                .Where(l => l.Status == "PendingReview")
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Listing>> GetPublishedListingsAsync()
        {
            return await _context.Listings
                .Include(l => l.Lister)
                .Include(nameof(Listing.ListingMedia))
                .Where(l => l.Status == "Published")
                .OrderByDescending(l => l.IsBoosted)
                .ThenByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _context.Listings.Include(l => l.Lister)
                                          .Include(nameof(Listing.ListingMedia))
                                          .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Listing?> GetListingWithMediaAsync(Guid id)
        {
            return await _context.Listings
                .Include(l => l.ListingMedia)
                //.Include(l => l.ListingTour360)
                .Include(l => l.Lister)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Listing>> GetListingsByListerIdAsync(Guid listerId)
        {
            return await _context.Listings
                .Where(l => l.ListerId == listerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Listing> Items, int TotalCount)> GetListingsFilteredAsync(
            Guid listerId,
            string? searchTerm,
            string? status,
            string? transactionType,
            string? propertyType,
            string? city,
            string? district,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string sortOrder,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Listings
                .Where(l => l.ListerId == listerId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(l =>
                    l.Title.ToLower().Contains(searchTerm) ||
                    (l.Description != null && l.Description.ToLower().Contains(searchTerm)) ||
                    (l.District != null && l.District.ToLower().Contains(searchTerm)) ||
                    (l.City != null && l.City.ToLower().Contains(searchTerm)));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            // Apply transaction type filter
            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                query = query.Where(l => l.TransactionType == transactionType);
            }

            // Apply property type filter
            if (!string.IsNullOrWhiteSpace(propertyType))
            {
                query = query.Where(l => l.PropertyType == propertyType);
            }

            // Apply city filter
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(l => l.City == city);
            }

            // Apply district filter
            if (!string.IsNullOrWhiteSpace(district))
            {
                query = query.Where(l => l.District == district);
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                query = query.Where(l => l.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.Price <= maxPrice.Value);
            }

            // Get total count before pagination
            int totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(l => l.Title) 
                    : query.OrderByDescending(l => l.Title),
                "price" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(l => l.Price) 
                    : query.OrderByDescending(l => l.Price),
                "status" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(l => l.Status) 
                    : query.OrderByDescending(l => l.Status),
                "updatedat" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(l => l.UpdatedAt) 
                    : query.OrderByDescending(l => l.UpdatedAt),
                _ => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(l => l.CreatedAt) 
                    : query.OrderByDescending(l => l.CreatedAt)
            };

            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Create
        public async Task<Listing> CreateAsync(Listing listing)
        {
            listing.Id = Guid.NewGuid();
            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;
            listing.Status = "Draft";

            await _context.Listings.AddAsync(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        // Update
        public async Task UpdateAsync(Listing listing)
        {
            listing.UpdatedAt = DateTime.UtcNow; 
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
        }

        // Delete (Hard delete)
        public async Task<bool> DeleteAsync(Guid id)
        {
            var listing = await _context.Listings
                .Include(l => l.ListingMedia)
                //.Include(l => l.ListingPriceHistories)
                //.Include(l => l.ListingTour360)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null) return false;

            // Remove related entities first
            if (listing.ListingMedia.Any())
                _context.ListingMedia.RemoveRange(listing.ListingMedia);

            if (listing.ListingPriceHistories.Any())
                _context.ListingPriceHistories.RemoveRange(listing.ListingPriceHistories);

            if (listing.ListingTour360 != null)
                _context.ListingTour360s.Remove(listing.ListingTour360);

            // Remove the listing
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();
            return true;
        }

        // Media Management
        public async Task AddMediaAsync(Guid listingId, ListingMedia media)
        {
            media.Id = Guid.NewGuid();
            media.ListingId = listingId;
            await _context.ListingMedia.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ListingMedia>> GetMediaByListingIdAsync(Guid listingId)
        {
            return await _context.ListingMedia
                .Where(m => m.ListingId == listingId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task DeleteMediaAsync(Guid mediaId)
        {
            var media = await _context.ListingMedia.FindAsync(mediaId);
            if (media != null)
            {
                _context.ListingMedia.Remove(media);
                await _context.SaveChangesAsync();
            }
        }

        // Validation Helpers
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Listings.AnyAsync(l => l.Id == id);
        }

        public async Task<bool> IsOwnerAsync(Guid listingId, Guid userId)
        {
            return await _context.Listings
                .AnyAsync(l => l.Id == listingId && l.ListerId == userId);
        }

        // Statistics Methods for Admin Dashboard
        public async Task<int> GetTotalListingsCountAsync()
        {
            return await _context.Listings.CountAsync();
        }

        public async Task<int> GetListingsCountByStatusAsync(string status)
        {
            return await _context.Listings.CountAsync(l => l.Status == status);
        }

        public async Task<decimal> GetAverageListingPriceAsync()
        {
            var listings = await _context.Listings.Where(l => l.Status == "Published").ToListAsync();
            if (!listings.Any()) return 0;
            return listings.Average(l => l.Price);
        }

        public async Task<int> GetBoostedListingsCountAsync()
        {
            return await _context.Listings.CountAsync(l => l.IsBoosted);
        }

        public async Task<Dictionary<string, int>> GetListingsCountByStatusAsync()
        {
            return await _context.Listings
                .GroupBy(l => l.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<List<(DateTime Date, int Count)>> GetListingsCreatedOverTimeAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            
            var listings = await _context.Listings
                .Where(l => l.CreatedAt >= startDate)
                .GroupBy(l => l.CreatedAt!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return listings.Select(x => (x.Date, x.Count)).ToList();
        }

        public async Task<List<(Guid ListingId, string Title, int LeadCount, decimal Price, string ListerName)>> GetTopPerformingListingsAsync(int topCount)
        {
            var topListings = await _context.Listings
                .Include(l => l.Lister)
                .Include(l => l.Leads)
                .Where(l => l.Status == "Published")
                .Select(l => new
                {
                    ListingId = l.Id,
                    Title = l.Title,
                    LeadCount = l.Leads.Count,
                    Price = l.Price,
                    ListerName = l.Lister.DisplayName
                })
                .OrderByDescending(x => x.LeadCount)
                .Take(topCount)
                .ToListAsync();

            return topListings.Select(x => (x.ListingId, x.Title, x.LeadCount, x.Price, x.ListerName)).ToList();
        }
    }
}
