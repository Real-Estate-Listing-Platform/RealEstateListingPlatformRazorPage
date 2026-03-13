using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ValuationModel : PageModel
    {
        [BindProperty]
        public ValuationRequestInput Input { get; set; } = new();

        public SelectList PropertyTypes { get; private set; } = null!;

        public IActionResult OnGet()
        {
            PopulatePropertyTypes();
            return Page();
        }

        public IActionResult OnPost()
        {
            PopulatePropertyTypes();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // RELP-54 focuses on FE request form only.
            // Until RELP-55 is ready, we keep submitted request in TempData for user feedback.
            TempData["ValuationSuccess"] =
                $"Da gui yeu cau dinh gia cho {Input.PropertyType} tai {Input.District}, {Input.City} ({Input.AreaSqm:0.##} m2).";

            return RedirectToPage();
        }

        private void PopulatePropertyTypes()
        {
            PropertyTypes = new SelectList(
                new[]
                {
                    new { Value = "Apartment", Text = "Can ho" },
                    new { Value = "House", Text = "Nha pho" },
                    new { Value = "Villa", Text = "Biet thu" },
                    new { Value = "Land", Text = "Dat" },
                    new { Value = "Commercial", Text = "Thuong mai" }
                },
                "Value",
                "Text");
        }

        public class ValuationRequestInput
        {
            [Required(ErrorMessage = "Vui long chon loai bat dong san.")]
            [Display(Name = "Loai bat dong san")]
            public string PropertyType { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui long nhap dien tich.")]
            [Range(1, 100000, ErrorMessage = "Dien tich phai lon hon 0.")]
            [Display(Name = "Dien tich (m2)")]
            public decimal AreaSqm { get; set; }

            [Required(ErrorMessage = "Vui long nhap tinh/thanh pho.")]
            [StringLength(100)]
            [Display(Name = "Tinh / Thanh pho")]
            public string City { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui long nhap quan/huyen.")]
            [StringLength(100)]
            [Display(Name = "Quan / Huyen")]
            public string District { get; set; } = string.Empty;

            [StringLength(100)]
            [Display(Name = "Phuong / Xa")]
            public string? Ward { get; set; }

            [StringLength(250)]
            [Display(Name = "Dia chi chi tiet")]
            public string? AddressLine { get; set; }

            [StringLength(500)]
            [Display(Name = "Ghi chu bo sung")]
            public string? Notes { get; set; }
        }
    }
}
