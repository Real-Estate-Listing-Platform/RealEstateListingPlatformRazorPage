using System.Security.Claims;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class CompareReportsModel : PageModel
    {
        private readonly IValuationReportService _reportService;

        public CompareReportsModel(IValuationReportService reportService)
        {
            _reportService = reportService;
        }

        public List<ValuationReportDto> Reports { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return RedirectToPage("/Lister/ValuationReports");

            var parsedIds = ids.Split(',')
                .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .Distinct()
                .ToList();

            if (parsedIds.Count < 2)
                return RedirectToPage("/Lister/ValuationReports");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Account/Login");

            Reports = await _reportService.GetForComparisonAsync(parsedIds, userId);

            if (Reports.Count < 2)
                return RedirectToPage("/Lister/ValuationReports");

            return Page();
        }
    }
}
