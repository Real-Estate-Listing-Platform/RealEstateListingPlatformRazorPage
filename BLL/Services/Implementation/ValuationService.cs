using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementation
{
    public class ValuationService : IValuationService
    {
        private const int MinSampleThreshold = 3;
        private const int MaxComparables = 5;

        private readonly IListingRepository _listingRepo;

        public ValuationService(IListingRepository listingRepo)
        {
            _listingRepo = listingRepo;
        }

        public async Task<ValuationResultDto> EstimateAsync(
            string propertyType,
            decimal areaSqm,
            string city,
            string district,
            string transactionType)
        {
            // 1. Try district-level first
            var listings = await _listingRepo.GetListingsForValuationAsync(
                propertyType, transactionType, city, district);

            bool isFallback = false;
            var priced = ExtractPricedSamples(listings);

            // 2. Fall back to city-level if not enough samples
            if (priced.Count < MinSampleThreshold)
            {
                listings = await _listingRepo.GetListingsForValuationAsync(
                    propertyType, transactionType, city, district: null);
                priced = ExtractPricedSamples(listings);
                isFallback = true;
            }

            if (priced.Count == 0)
            {
                return new ValuationResultDto
                {
                    HasData = false,
                    PropertyType = propertyType,
                    TransactionType = transactionType,
                    City = city,
                    District = district,
                    AreaSqm = areaSqm,
                    MarketInsight = $"Chưa có đủ dữ liệu tin đăng đã duyệt tại {city} cho loại \"{propertyType}\" ({transactionType}). Vui lòng thử lại sau.",
                    IsFallbackToCity = isFallback
                };
            }

            // 3. Compute statistics
            var avgPricePerSqm = priced.Average(p => p.PricePerSqm);
            var minPricePerSqm = priced.Min(p => p.PricePerSqm);
            var maxPricePerSqm = priced.Max(p => p.PricePerSqm);

            var estimatedAvg = avgPricePerSqm * areaSqm;
            var estimatedMin = minPricePerSqm * areaSqm;
            var estimatedMax = maxPricePerSqm * areaSqm;

            var location = isFallback ? city : $"{district}, {city}";
            var insight = BuildInsight(priced.Count, avgPricePerSqm, areaSqm,
                estimatedAvg, location, transactionType, isFallback);

            // 4. Top comparables to display
            var comparables = priced
                .OrderByDescending(p => p.Listing.CreatedAt)
                .Take(MaxComparables)
                .Select(p => new ComparableListingDto
                {
                    Id = p.Listing.Id,
                    Title = p.Listing.Title,
                    Price = p.Listing.Price,
                    AreaSqm = p.AreaSqm,
                    PricePerSqm = p.PricePerSqm,
                    District = p.Listing.District,
                    City = p.Listing.City,
                    TransactionType = p.Listing.TransactionType,
                    CreatedAt = p.Listing.CreatedAt
                })
                .ToList();

            return new ValuationResultDto
            {
                HasData = true,
                PropertyType = propertyType,
                TransactionType = transactionType,
                City = city,
                District = district,
                AreaSqm = areaSqm,
                AvgPricePerSqm = avgPricePerSqm,
                MinPricePerSqm = minPricePerSqm,
                MaxPricePerSqm = maxPricePerSqm,
                EstimatedAvgPrice = estimatedAvg,
                EstimatedMinPrice = estimatedMin,
                EstimatedMaxPrice = estimatedMax,
                SampleCount = priced.Count,
                IsFallbackToCity = isFallback,
                MarketInsight = insight,
                ComparableListings = comparables
            };
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private record PricedSample(Listing Listing, decimal AreaSqm, decimal PricePerSqm);

        private static List<PricedSample> ExtractPricedSamples(List<Listing> listings)
        {
            var result = new List<PricedSample>();
            foreach (var l in listings)
            {
                var area = ParseArea(l.Area);
                if (area is > 0 && l.Price > 0)
                    result.Add(new PricedSample(l, area.Value, l.Price / area.Value));
            }
            return result;
        }

        /// <summary>
        /// Parses area strings like "65", "65m2", "65 m²", "65.5" → decimal?
        /// </summary>
        private static decimal? ParseArea(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var digits = Regex.Match(raw, @"[\d]+([.,][\d]+)?");
            if (!digits.Success) return null;
            var normalized = digits.Value.Replace(',', '.');
            return decimal.TryParse(normalized,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var val) ? val : null;
        }

        private static string BuildInsight(int count, decimal avgPerSqm, decimal areaSqm,
            decimal? estimated, string location, string txType, bool isFallback)
        {
            var priceLabel = txType.Equals("Rent", StringComparison.OrdinalIgnoreCase) ? "thuê" : "bán";
            var fallbackNote = isFallback
                ? " (dữ liệu mở rộng toàn thành phố do quận/huyện chưa đủ mẫu)"
                : "";

            return $"Dựa trên {count} tin đăng {priceLabel} đã duyệt tại {location}{fallbackNote}, " +
                   $"giá trung bình là {avgPerSqm:N0} VNĐ/m². " +
                   $"Với diện tích {areaSqm:0.##} m², " +
                   $"ước tính giá trị khoảng {estimated:N0} VNĐ.";
        }
    }
}
