using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    /// <summary>
    /// REST API for market data aggregation and trend analysis.
    /// All endpoints return JSON and require no authentication so that
    /// external consumers (mobile apps, BI tools) can query the data.
    /// </summary>
    [Route("api/market")]
    [ApiController]
    public class MarketDataController : ControllerBase
    {
        private readonly IMarketAnalyticsService _analyticsService;

        public MarketDataController(IMarketAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // ── GET /api/market/full ──────────────────────────────────────────────
        /// <summary>
        /// Returns the complete analytics payload: summary, trend, district
        /// stats, and property-type distribution in one call.
        /// </summary>
        [HttpGet("full")]
        public async Task<IActionResult> GetFull(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            if (months is < 1 or > 60)
                return BadRequest(Fail("months must be between 1 and 60."));

            var data = await _analyticsService.GetAnalyticsAsync(city, propertyType, transactionType, months);
            return Ok(Ok(data));
        }

        // ── GET /api/market/trends ────────────────────────────────────────────
        /// <summary>Monthly avg price/m² per district (top-5 districts).</summary>
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetAnalyticsAsync(city, propertyType, transactionType, months);
            return Ok(Success(data.PriceTrend));
        }

        // ── GET /api/market/districts ─────────────────────────────────────────
        /// <summary>Current avg/min/max price/m² for every district in the city.</summary>
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetAnalyticsAsync(city, propertyType, transactionType, months);
            return Ok(Success(data.DistrictStats));
        }

        // ── GET /api/market/summary ───────────────────────────────────────────
        /// <summary>High-level market summary numbers (stat cards).</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetAnalyticsAsync(city, propertyType, transactionType, months);
            return Ok(Success(data.Summary));
        }

        // ── GET /api/market/hotspots ──────────────────────────────────────────
        /// <summary>
        /// Districts with the highest price-per-m² growth between the first
        /// and second half of the requested period.
        /// </summary>
        [HttpGet("hotspots")]
        public async Task<IActionResult> GetHotspots(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12,
            [FromQuery] int topN = 5)
        {
            if (topN is < 1 or > 20)
                return BadRequest(Fail("topN must be between 1 and 20."));

            var hotspots = await _analyticsService.GetHotspotsAsync(
                city, propertyType, transactionType, months, topN);

            return Ok(Success(hotspots));
        }

        // ── GET /api/market/price-ranges ──────────────────────────────────────
        /// <summary>
        /// How listings are distributed across total-price brackets
        /// (0–1B, 1–3B, 3–5B, 5–10B, 10–20B, 20B+).
        /// </summary>
        [HttpGet("price-ranges")]
        public async Task<IActionResult> GetPriceRanges(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            var buckets = await _analyticsService.GetPriceRangeDistributionAsync(
                city, propertyType, transactionType, months);

            return Ok(Success(buckets));
        }

        // ── GET /api/market/city-comparison ───────────────────────────────────
        /// <summary>
        /// Side-by-side avg price/m², median price, listing count, and MoM
        /// change across up to 10 cities.
        /// Pass multiple values: ?cities=Hà Nội&cities=TP. Hồ Chí Minh
        /// </summary>
        [HttpGet("city-comparison")]
        public async Task<IActionResult> GetCityComparison(
            [FromQuery] List<string> cities,
            [FromQuery] string? propertyType = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            if (cities == null || !cities.Any())
                return BadRequest(Fail("Provide at least one city via ?cities=..."));

            if (cities.Count > 10)
                return BadRequest(Fail("Maximum 10 cities per request."));

            var result = await _analyticsService.GetCityComparisonAsync(
                cities, propertyType, transactionType, months);

            return Ok(Success(result));
        }

        // ── GET /api/market/type-distribution ─────────────────────────────────
        /// <summary>Listing count broken down by property type.</summary>
        [HttpGet("type-distribution")]
        public async Task<IActionResult> GetTypeDistribution(
            [FromQuery] string city = "Hồ Chí Minh",
            [FromQuery] string? transactionType = null,
            [FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetAnalyticsAsync(city, null, transactionType, months);
            return Ok(Success(data.TypeDistribution));
        }

        // ── helpers ───────────────────────────────────────────────────────────
        private static MarketApiResponse<T> Success<T>(T data, string? msg = null) =>
            new() { Success = true, Data = data, Message = msg, GeneratedAt = DateTime.UtcNow };

        private static MarketApiResponse<object> Fail(string message) =>
            new() { Success = false, Message = message, GeneratedAt = DateTime.UtcNow };
    }
}
