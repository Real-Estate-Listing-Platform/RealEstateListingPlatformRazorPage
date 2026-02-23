using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize]
    public class RetryPaymentModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public RetryPaymentModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnPostAsync(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToPage("/Payment/MyTransactions");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToPage("/Payment/MyTransactions");
            }

            // Only allow retry for Pending or Failed transactions
            if (transaction.Data.Status != "Pending" && transaction.Data.Status != "Failed")
            {
                TempData["Error"] = "This transaction cannot be retried";
                return RedirectToPage("/Payment/MyTransactions");
            }

            // Create new PayOS payment link
            var paymentResult = await _paymentService.InitiatePaymentAsync(
                transactionId,
                transaction.Data.PaymentMethod ?? "PAYOS"
            );

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.Data))
            {
                TempData["Error"] = paymentResult.Message ?? "Failed to create payment link";
                return RedirectToPage("/Payment/MyTransactions");
            }

            // Redirect to PayOS payment page
            return Redirect(paymentResult.Data);
        }
    }
}
