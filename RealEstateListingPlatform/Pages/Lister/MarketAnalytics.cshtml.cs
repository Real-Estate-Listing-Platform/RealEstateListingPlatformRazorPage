using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class MarketAnalyticsModel : PageModel
    {
        private readonly IMarketAnalyticsService _analyticsService;

        public MarketAnalyticsModel(IMarketAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // ── Filter inputs (GET params) ────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public string City { get; set; } = "Hồ Chí Minh";

        [BindProperty(SupportsGet = true)]
        public string? PropertyType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TransactionType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Months { get; set; } = 12;

        // ── Data for the view ─────────────────────────────────────────────────
        public MarketAnalyticsResultDto? Result { get; private set; }

        // Pre-serialised JSON strings injected into Chart.js
        public string TrendLabelsJson { get; private set; } = "[]";
        public string TrendDatasetsJson { get; private set; } = "[]";
        public string DistrictLabelsJson { get; private set; } = "[]";
        public string DistrictAvgJson { get; private set; } = "[]";
        public string DistrictMinJson { get; private set; } = "[]";
        public string DistrictMaxJson { get; private set; } = "[]";
        public string TypeLabelsJson { get; private set; } = "[]";
        public string TypeCountsJson { get; private set; } = "[]";

        // Dropdowns
        public SelectList PropertyTypes { get; private set; } = null!;
        public SelectList TransactionTypes { get; private set; } = null!;
        public SelectList MonthOptions { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            PopulateDropdowns();

            Result = await _analyticsService.GetAnalyticsAsync(
                City.Trim(), PropertyType, TransactionType, Months);

            BuildChartData(Result);
            return Page();
        }

        private void BuildChartData(MarketAnalyticsResultDto r)
        {
            var opts = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

            // ── Trend line chart ──────────────────────────────────────────────
            // Build unique sorted month labels
            var monthKeys = r.PriceTrend
                .Select(p => new { p.Year, p.Month })
                .Distinct()
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            TrendLabelsJson = JsonSerializer.Serialize(
                monthKeys.Select(m => $"{m.Month:D2}/{m.Year}").ToArray(), opts);

            var districts = r.PriceTrend.Select(p => p.District).Distinct().OrderBy(d => d).ToList();
            var palette = new[] { "#0d6efd", "#dc3545", "#198754", "#ffc107", "#6f42c1" };

            var datasets = districts.Select((district, i) =>
            {
                var color = palette[i % palette.Length];
                var data = monthKeys.Select(mk =>
                {
                    var pt = r.PriceTrend.FirstOrDefault(p =>
                        p.District == district && p.Year == mk.Year && p.Month == mk.Month);
                    return pt?.AvgPricePerSqm;
                }).ToArray();

                return new
                {
                    label = district,
                    data,
                    borderColor = color,
                    backgroundColor = color + "22",
                    borderWidth = 2,
                    pointRadius = 4,
                    tension = 0.3,
                    fill = false,
                    spanGaps = true
                };
            }).ToList();

            TrendDatasetsJson = JsonSerializer.Serialize(datasets, opts);

            // ── District bar chart ────────────────────────────────────────────
            DistrictLabelsJson = JsonSerializer.Serialize(r.DistrictStats.Select(d => d.District).ToArray(), opts);
            DistrictAvgJson    = JsonSerializer.Serialize(r.DistrictStats.Select(d => d.AvgPricePerSqm).ToArray(), opts);
            DistrictMinJson    = JsonSerializer.Serialize(r.DistrictStats.Select(d => d.MinPricePerSqm).ToArray(), opts);
            DistrictMaxJson    = JsonSerializer.Serialize(r.DistrictStats.Select(d => d.MaxPricePerSqm).ToArray(), opts);

            // ── Type doughnut ─────────────────────────────────────────────────
            TypeLabelsJson = JsonSerializer.Serialize(r.TypeDistribution.Select(t => t.PropertyType).ToArray(), opts);
            TypeCountsJson = JsonSerializer.Serialize(r.TypeDistribution.Select(t => t.Count).ToArray(), opts);
        }

        private void PopulateDropdowns()
        {
            PropertyTypes = new SelectList(new[]
            {
                new { Value = "",           Text = "Tất cả loại" },
                new { Value = "Apartment",  Text = "Căn hộ" },
                new { Value = "House",      Text = "Nhà phố" },
                new { Value = "Villa",      Text = "Biệt thự" },
                new { Value = "Land",       Text = "Đất" },
                new { Value = "Commercial", Text = "Thương mại" }
            }, "Value", "Text", PropertyType);

            TransactionTypes = new SelectList(new[]
            {
                new { Value = "",     Text = "Tất cả hình thức" },
                new { Value = "Sell", Text = "Mua bán" },
                new { Value = "Rent", Text = "Cho thuê" }
            }, "Value", "Text", TransactionType);

            MonthOptions = new SelectList(new[]
            {
                new { Value = 3,  Text = "3 tháng gần nhất" },
                new { Value = 6,  Text = "6 tháng gần nhất" },
                new { Value = 12, Text = "12 tháng gần nhất" },
                new { Value = 24, Text = "24 tháng gần nhất" }
            }, "Value", "Text", Months);
        }
    }
}
