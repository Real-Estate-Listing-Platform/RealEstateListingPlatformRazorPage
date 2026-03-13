using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementation
{
    public class MarketAnalyticsService : IMarketAnalyticsService
    {
        private readonly IListingRepository _listingRepo;

        public MarketAnalyticsService(IListingRepository listingRepo)
        {
            _listingRepo = listingRepo;
        }

        public async Task<MarketAnalyticsResultDto> GetAnalyticsAsync(
            string city,
            string? propertyType,
            string? transactionType,
            int months = 12)
        {
            var listings = await _listingRepo.GetListingsForMarketAnalyticsAsync(
                city, propertyType, transactionType, months);

            // Attach a parsed area to each listing in memory
            var priced = listings
                .Select(l => new { Listing = l, AreaSqm = ParseArea(l.Area) })
                .Where(x => x.AreaSqm is > 0 && x.Listing.Price > 0)
                .Select(x => new
                {
                    x.Listing,
                    AreaSqm = x.AreaSqm!.Value,
                    PricePerSqm = x.Listing.Price / x.AreaSqm!.Value
                })
                .ToList();

            // ── Price trend (monthly × district) ─────────────────────────────
            var trend = priced
                .Where(x => x.Listing.CreatedAt.HasValue && !string.IsNullOrWhiteSpace(x.Listing.District))
                .GroupBy(x => new
                {
                    x.Listing.CreatedAt!.Value.Year,
                    x.Listing.CreatedAt!.Value.Month,
                    District = x.Listing.District!
                })
                .Select(g => new PriceTrendPointDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    District = g.Key.District,
                    AvgPricePerSqm = g.Average(x => x.PricePerSqm),
                    ListingCount = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Only keep top-5 districts by total listing count (to avoid cluttered chart)
            var top5Districts = priced
                .Where(x => !string.IsNullOrWhiteSpace(x.Listing.District))
                .GroupBy(x => x.Listing.District!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToHashSet();

            trend = trend.Where(t => top5Districts.Contains(t.District)).ToList();

            // ── District stats (current snapshot) ────────────────────────────
            var districtStats = priced
                .Where(x => !string.IsNullOrWhiteSpace(x.Listing.District))
                .GroupBy(x => x.Listing.District!)
                .Select(g => new DistrictPriceStatDto
                {
                    District = g.Key,
                    AvgPricePerSqm = g.Average(x => x.PricePerSqm),
                    MinPricePerSqm = g.Min(x => x.PricePerSqm),
                    MaxPricePerSqm = g.Max(x => x.PricePerSqm),
                    ListingCount = g.Count()
                })
                .OrderByDescending(d => d.ListingCount)
                .Take(10)
                .ToList();

            // ── Type distribution (all listings, not just priced) ─────────────
            var typeDist = listings
                .Where(l => !string.IsNullOrWhiteSpace(l.PropertyType))
                .GroupBy(l => l.PropertyType!)
                .Select(g => new PropertyTypeCountDto
                {
                    PropertyType = MapTypeName(g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ── Summary ───────────────────────────────────────────────────────
            var topDistrict = districtStats.FirstOrDefault();
            decimal? momChange = ComputeMomChange(trend);

            var summary = new MarketSummaryDto
            {
                TotalListings = listings.Count,
                AvgPricePerSqm = priced.Any() ? priced.Average(x => x.PricePerSqm) : 0,
                TopDistrict = topDistrict?.District ?? "–",
                TopDistrictCount = topDistrict?.ListingCount ?? 0,
                PriceChangePct = momChange
            };

            return new MarketAnalyticsResultDto
            {
                Summary = summary,
                PriceTrend = trend,
                DistrictStats = districtStats,
                TypeDistribution = typeDist,
                City = city,
                PropertyType = propertyType,
                TransactionType = transactionType,
                Months = months
            };
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static decimal? ParseArea(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var m = Regex.Match(raw, @"[\d]+([.,][\d]+)?");
            if (!m.Success) return null;
            var normalized = m.Value.Replace(',', '.');
            return decimal.TryParse(normalized,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var val) ? val : null;
        }

        private static string MapTypeName(string raw) => raw switch
        {
            "Apartment"  => "Căn hộ",
            "House"      => "Nhà phố",
            "Villa"      => "Biệt thự",
            "Land"       => "Đất",
            "Commercial" => "Thương mại",
            _            => raw
        };

        /// <summary>Month-over-month change % of avg price/m² across all districts.</summary>
        private static decimal? ComputeMomChange(List<PriceTrendPointDto> trend)
        {
            if (!trend.Any()) return null;

            var byMonth = trend
                .GroupBy(t => new { t.Year, t.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(2)
                .ToList();

            if (byMonth.Count < 2) return null;

            var currentAvg = byMonth[0].Average(t => t.AvgPricePerSqm);
            var prevAvg    = byMonth[1].Average(t => t.AvgPricePerSqm);

            if (prevAvg == 0) return null;
            return Math.Round((currentAvg - prevAvg) / prevAvg * 100, 1);
        }
    }
}
