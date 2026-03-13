using BLL.DTOs;

namespace BLL.Services
{
    public interface IMarketAnalyticsService
    {
        /// <summary>
        /// Returns the full market analytics payload for the given filters.
        /// </summary>
        Task<MarketAnalyticsResultDto> GetAnalyticsAsync(
            string city,
            string? propertyType,
            string? transactionType,
            int months = 12);
    }
}
