using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPackageService _packageService;

        public IndexModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        public List<PackageDto> Packages { get; set; } = new();
        public string? PackageType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ServiceResult<List<PackageDto>> result;

            if (!string.IsNullOrEmpty(Type))
            {
                result = await _packageService.GetPackagesByTypeAsync(Type);
                PackageType = Type;
            }
            else
            {
                result = await _packageService.GetActivePackagesAsync();
            }

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                Packages = new List<PackageDto>();
                return Page();
            }

            Packages = result.Data ?? new List<PackageDto>();
            return Page();
        }
    }
}
