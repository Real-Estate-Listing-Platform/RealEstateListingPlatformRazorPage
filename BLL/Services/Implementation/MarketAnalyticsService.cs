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

            var priced = ToPricedSamples(listings);

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

            // ── Type distribution ─────────────────────────────────────────────
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

        // ── RELP-57: additional aggregation methods ───────────────────────────

        public async Task<List<HotspotDto>> GetHotspotsAsync(
            string city, string? propertyType, string? transactionType,
            int months = 12, int topN = 5)
        {
            var listings = await _listingRepo.GetListingsForMarketAnalyticsAsync(
                city, propertyType, transactionType, months);

            var priced = ToPricedSamples(listings);
            if (!priced.Any()) return new List<HotspotDto>();

            // Split period in half: "previous" vs "current"
            var midpoint = DateTime.UtcNow.AddMonths(-(months / 2));

            var byDistrict = priced
                .Where(x => !string.IsNullOrWhiteSpace(x.Listing.District)
                             && x.Listing.CreatedAt.HasValue)
                .GroupBy(x => x.Listing.District!)
                .Select(g =>
                {
                    var prev = g.Where(x => x.Listing.CreatedAt < midpoint).ToList();
                    var curr = g.Where(x => x.Listing.CreatedAt >= midpoint).ToList();

                    if (!prev.Any() || !curr.Any()) return null;

                    var prevAvg = prev.Average(x => x.PricePerSqm);
                    var currAvg = curr.Average(x => x.PricePerSqm);
                    var growth  = prevAvg == 0 ? 0 : Math.Round((currAvg - prevAvg) / prevAvg * 100, 1);

                    return new HotspotDto
                    {
                        District = g.Key,
                        City = city,
                        CurrentAvgPricePerSqm  = currAvg,
                        PreviousAvgPricePerSqm = prevAvg,
                        GrowthPct  = growth,
                        ListingCount = g.Count()
                    };
                })
                .Where(x => x != null)
                .Select(x => x!)
                .OrderByDescending(x => x.GrowthPct)
                .Take(topN)
                .ToList();

            return byDistrict;
        }

        public async Task<List<PriceRangeBucketDto>> GetPriceRangeDistributionAsync(
            string city, string? propertyType, string? transactionType, int months = 12)
        {
            var listings = await _listingRepo.GetListingsForMarketAnalyticsAsync(
                city, propertyType, transactionType, months);

            if (!listings.Any()) return new List<PriceRangeBucketDto>();

            // Buckets in VNĐ (billion)
            var buckets = new[]
            {
                (Label: "Dưới 1 tỷ",    Min: 0m,          Max: 1_000_000_000m),
                (Label: "1 – 3 tỷ",     Min: 1e9m,        Max: 3_000_000_000m),
                (Label: "3 – 5 tỷ",     Min: 3e9m,        Max: 5_000_000_000m),
                (Label: "5 – 10 tỷ",    Min: 5e9m,        Max: 10_000_000_000m),
                (Label: "10 – 20 tỷ",   Min: 10e9m,       Max: 20_000_000_000m),
                (Label: "Trên 20 tỷ",   Min: 20e9m,       Max: decimal.MaxValue)
            };

            var total = listings.Count;
            return buckets.Select(b =>
            {
                var count = listings.Count(l => l.Price >= b.Min && l.Price < b.Max);
                return new PriceRangeBucketDto
                {
                    Label      = b.Label,
                    MinPrice   = b.Min,
                    MaxPrice   = b.Max == decimal.MaxValue ? 0 : b.Max,
                    Count      = count,
                    Percentage = total > 0 ? Math.Round((decimal)count / total * 100, 1) : 0
                };
            }).ToList();
        }

        public async Task<List<CityComparisonDto>> GetCityComparisonAsync(
            IEnumerable<string> cities, string? propertyType,
            string? transactionType, int months = 12)
        {
            var cityList = cities.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var allListings = await _listingRepo.GetListingsForCityComparisonAsync(
                cityList, propertyType, transactionType, months);

            var result = new List<CityComparisonDto>();

            foreach (var city in cityList)
            {
                var cityListings = allListings
                    .Where(l => l.City != null && l.City.Contains(city, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var priced = ToPricedSamples(cityListings);
                if (!priced.Any())
                {
                    result.Add(new CityComparisonDto { City = city, ListingCount = 0 });
                    continue;
                }

                var sorted  = priced.OrderBy(x => x.PricePerSqm).ToList();
                var median  = sorted[sorted.Count / 2].PricePerSqm;
                var trend   = priced
                    .Where(x => x.Listing.CreatedAt.HasValue)
                    .GroupBy(x => new { x.Listing.CreatedAt!.Value.Year, x.Listing.CreatedAt!.Value.Month })
                    .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                    .Take(2).ToList();

                decimal? mom = null;
                if (trend.Count == 2)
                {
                    var curr = trend[0].Average(x => x.PricePerSqm);
                    var prev = trend[1].Average(x => x.PricePerSqm);
                    if (prev > 0) mom = Math.Round((curr - prev) / prev * 100, 1);
                }

                result.Add(new CityComparisonDto
                {
                    City             = city,
                    AvgPricePerSqm   = priced.Average(x => x.PricePerSqm),
                    MedianPrice      = median,
                    ListingCount     = priced.Count,
                    MomChangePct     = mom
                });
            }

            return result.OrderByDescending(c => c.AvgPricePerSqm).ToList();
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private record PricedListing(Listing Listing, decimal AreaSqm, decimal PricePerSqm);

        private static List<PricedListing> ToPricedSamples(List<Listing> listings) =>
            listings
                .Select(l => new { Listing = l, Area = ParseArea(l.Area) })
                .Where(x => x.Area is > 0 && x.Listing.Price > 0)
                .Select(x => new PricedListing(x.Listing, x.Area!.Value, x.Listing.Price / x.Area!.Value))
                .ToList();

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
