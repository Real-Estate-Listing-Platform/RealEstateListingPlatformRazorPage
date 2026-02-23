using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize]
    public class ProcessModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public ProcessModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid TransactionId { get; set; }

        public TransactionDto? Transaction { get; set; }
        public string? CheckoutUrl { get; set; }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(TransactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToPage("/Package/Index");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToPage("/Package/Index");
            }

            // Only pending transactions can be processed
            if (transaction.Data.Status != "Pending")
            {
                if (transaction.Data.Status == "Completed")
                {
                    return RedirectToPage("/Payment/Success", new { transactionId = TransactionId });
                }
                TempData["Error"] = "This transaction cannot be processed";
                return RedirectToPage("/Package/Index");
            }

            // Get or create PayOS payment link
            var paymentResult = await _paymentService.InitiatePaymentAsync(
                TransactionId,
                transaction.Data.PaymentMethod ?? "PAYOS"
            );

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.Data))
            {
                TempData["Error"] = paymentResult.Message ?? "Failed to create payment link";
                return RedirectToPage("/Package/Index");
            }

            CheckoutUrl = paymentResult.Data;
            Transaction = transaction.Data;
            return Page();
        }
    }
}
