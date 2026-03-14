using System.Security.Claims;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ValuationReportsModel : PageModel
    {
        private readonly IValuationReportService _reportService;

        public ValuationReportsModel(IValuationReportService reportService)
        {
            _reportService = reportService;
        }

        public List<ValuationReportDto> Reports { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public List<Guid> SelectedIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return RedirectToPage("/Account/Login");

            Reports = await _reportService.GetMyReportsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var userId = GetUserId();
            if (userId != Guid.Empty)
                await _reportService.DeleteAsync(id, userId);

            return RedirectToPage();
        }

        public IActionResult OnPostCompare()
        {
            if (SelectedIds.Count < 2)
            {
                TempData["CompareError"] = "Vui lòng chọn ít nhất 2 báo cáo để so sánh.";
                return RedirectToPage();
            }
            if (SelectedIds.Count > 4)
            {
                TempData["CompareError"] = "Chỉ có thể so sánh tối đa 4 báo cáo cùng lúc.";
                return RedirectToPage();
            }

            var ids = string.Join(",", SelectedIds);
            return RedirectToPage("/Lister/CompareReports", new { ids });
        }

        private Guid GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : Guid.Empty;
        }
    }
}
