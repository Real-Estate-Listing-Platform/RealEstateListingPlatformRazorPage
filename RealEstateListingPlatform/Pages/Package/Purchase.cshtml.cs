using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Package
{
    [Authorize]
    public class PurchaseModel : PageModel
    {
        private readonly IPackageService _packageService;
        private readonly IPaymentService _paymentService;

        public PurchaseModel(IPackageService packageService, IPaymentService paymentService)
        {
            _packageService = packageService;
            _paymentService = paymentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public Guid PackageId { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; } = "Transfer";

        [BindProperty]
        public string? Notes { get; set; }

        public PackageDto? PackageData { get; set; }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _packageService.GetPackageByIdAsync(Id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Package not found";
                return RedirectToPage("/Package/Index");
            }

            PackageData = result.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userId = GetCurrentUserId();

                var purchaseDto = new PurchasePackageDto
                {
                    PackageId = PackageId,
                    PaymentMethod = PaymentMethod,
                    Notes = Notes
                };

                var result = await _packageService.PurchasePackageAsync(userId, purchaseDto);

                if (!result.Success)
                {
                    TempData["Error"] = $"Purchase failed: {result.Message}";
                    return RedirectToPage("/Package/Index");
                }

                // Ensure transaction data is available
                if (result.Data == null)
                {
                    TempData["Error"] = "Failed to create transaction data";
                    return RedirectToPage("/Package/Index");
                }

                var transactionId = result.Data.TransactionId;

                // Create PayOS payment link
                var paymentResult = await _paymentService.InitiatePaymentAsync(transactionId, PaymentMethod);

                if (!paymentResult.Success)
                {
                    TempData["Error"] = $"Payment initiation failed: {paymentResult.Message ?? "Unknown error"}";
                    return RedirectToPage("/Package/Index");
                }

                // Redirect to Payment Process page to show QR code
                return RedirectToPage("/Payment/Process", new { transactionId = transactionId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToPage("/Package/Index");
            }
        }
    }
}
