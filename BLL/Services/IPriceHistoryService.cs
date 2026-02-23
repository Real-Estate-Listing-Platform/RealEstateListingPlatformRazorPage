using DAL.Models;

namespace BLL.Services
{
    public interface IPriceHistoryService
    {
        Task RecordPriceChangeAsync(Guid listingId, decimal oldPrice, decimal newPrice, Guid changedBy);
        Task<List<ListingPriceHistory>> GetPriceHistoryAsync(Guid listingId);
    }
}
