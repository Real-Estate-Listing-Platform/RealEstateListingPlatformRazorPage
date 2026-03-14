using BLL.DTOs;

namespace BLL.Services
{
    public interface IMarketAnalyticsService
    {
        /// <summary>Full analytics payload (used by the FE dashboard).</summary>
        Task<MarketAnalyticsResultDto> GetAnalyticsAsync(
            string city,
            string? propertyType,
            string? transactionType,
            int months = 12);

        /// <summary>
        /// Top N districts ranked by price growth between the first and last
        /// half of the requested period.
        /// </summary>
        Task<List<HotspotDto>> GetHotspotsAsync(
            string city,
            string? propertyType,
            string? transactionType,
            int months = 12,
            int topN = 5);

        /// <summary>Distribution of listings across predefined total-price brackets.</summary>
        Task<List<PriceRangeBucketDto>> GetPriceRangeDistributionAsync(
            string city,
            string? propertyType,
            string? transactionType,
            int months = 12);

        /// <summary>Side-by-side avg price/m² comparison across multiple cities.</summary>
        Task<List<CityComparisonDto>> GetCityComparisonAsync(
            IEnumerable<string> cities,
            string? propertyType,
            string? transactionType,
            int months = 12);
    }
}
