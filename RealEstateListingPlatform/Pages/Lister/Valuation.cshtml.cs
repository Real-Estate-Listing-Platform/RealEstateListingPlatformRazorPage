using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ValuationModel : PageModel
    {
        private readonly IValuationService _valuationService;
        private readonly IValuationReportService _reportService;

        public ValuationModel(IValuationService valuationService,
                              IValuationReportService reportService)
        {
            _valuationService = valuationService;
            _reportService    = reportService;
        }

        [BindProperty]
        public ValuationRequestInput Input { get; set; } = new();

        public SelectList PropertyTypes { get; private set; } = null!;
        public SelectList TransactionTypes { get; private set; } = null!;

        /// <summary>Populated after a successful POST with the estimation result.</summary>
        public ValuationResultDto? EstimationResult { get; private set; }

        public bool SavedSuccessfully { get; private set; }
        public string? SavedReportId { get; private set; }

        public IActionResult OnGet()
        {
            PopulateDropdowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PopulateDropdowns();

            if (!ModelState.IsValid)
                return Page();

            EstimationResult = await _valuationService.EstimateAsync(
                Input.PropertyType,
                Input.AreaSqm,
                Input.City.Trim(),
                Input.District.Trim(),
                Input.TransactionType);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            PopulateDropdowns();

            if (!ModelState.IsValid)
                return Page();

            EstimationResult = await _valuationService.EstimateAsync(
                Input.PropertyType,
                Input.AreaSqm,
                Input.City.Trim(),
                Input.District.Trim(),
                Input.TransactionType);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Page();

            var dto = new SaveReportDto
            {
                ReportName        = Input.ReportName,
                PropertyType      = Input.PropertyType,
                TransactionType   = Input.TransactionType,
                AreaSqm           = Input.AreaSqm,
                City              = Input.City.Trim(),
                District          = Input.District.Trim(),
                Ward              = Input.Ward,
                AddressLine       = Input.AddressLine,
                Notes             = Input.Notes,
                EstimatedMinPrice = EstimationResult?.EstimatedMinPrice,
                EstimatedAvgPrice = EstimationResult?.EstimatedAvgPrice,
                EstimatedMaxPrice = EstimationResult?.EstimatedMaxPrice,
                AvgPricePerSqm    = EstimationResult?.AvgPricePerSqm,
                SampleCount       = EstimationResult?.SampleCount ?? 0,
                IsFallbackToCity  = EstimationResult?.IsFallbackToCity ?? false,
                MarketInsight     = EstimationResult?.MarketInsight ?? string.Empty
            };

            var saved = await _reportService.SaveAsync(userId, dto);
            SavedSuccessfully = true;
            SavedReportId     = saved.Id.ToString();

            return Page();
        }

        private void PopulateDropdowns()
        {
            PropertyTypes = new SelectList(
                new[]
                {
                    new { Value = "Apartment",  Text = "Căn hộ" },
                    new { Value = "House",       Text = "Nhà phố" },
                    new { Value = "Villa",       Text = "Biệt thự" },
                    new { Value = "Land",        Text = "Đất" },
                    new { Value = "Commercial",  Text = "Thương mại" }
                },
                "Value", "Text");

            TransactionTypes = new SelectList(
                new[]
                {
                    new { Value = "Sell", Text = "Mua bán" },
                    new { Value = "Rent", Text = "Cho thuê" }
                },
                "Value", "Text");
        }

        public class ValuationRequestInput
        {
            [StringLength(200)]
            [Display(Name = "Tên báo cáo (tuỳ chọn)")]
            public string? ReportName { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn loại bất động sản.")]
            [Display(Name = "Loại bất động sản")]
            public string PropertyType { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn hình thức giao dịch.")]
            [Display(Name = "Hình thức")]
            public string TransactionType { get; set; } = "Sell";

            [Required(ErrorMessage = "Vui lòng nhập diện tích.")]
            [Range(1, 100000, ErrorMessage = "Diện tích phải lớn hơn 0.")]
            [Display(Name = "Diện tích (m²)")]
            public decimal AreaSqm { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
            [StringLength(100)]
            [Display(Name = "Tỉnh / Thành phố")]
            public string City { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập quận/huyện.")]
            [StringLength(100)]
            [Display(Name = "Quận / Huyện")]
            public string District { get; set; } = string.Empty;

            [StringLength(100)]
            [Display(Name = "Phường / Xã")]
            public string? Ward { get; set; }

            [StringLength(250)]
            [Display(Name = "Địa chỉ chi tiết")]
            public string? AddressLine { get; set; }

            [StringLength(500)]
            [Display(Name = "Ghi chú bổ sung")]
            public string? Notes { get; set; }
        }
    }
}
