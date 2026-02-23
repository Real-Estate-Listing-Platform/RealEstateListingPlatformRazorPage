using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementation
{
    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly RealEstateListingPlatformContext _context;

        public PriceHistoryService(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task RecordPriceChangeAsync(Guid listingId, decimal oldPrice, decimal newPrice, Guid changedBy)
        {
            var priceHistory = new ListingPriceHistory
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangedByUserId = changedBy,
                ChangedAt = DateTime.UtcNow
            };

            await _context.ListingPriceHistories.AddAsync(priceHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ListingPriceHistory>> GetPriceHistoryAsync(Guid listingId)
        {
            return await _context.ListingPriceHistories
                .Where(ph => ph.ListingId == listingId)
                .OrderByDescending(ph => ph.ChangedAt)
                .ToListAsync();
        }
    }
}
