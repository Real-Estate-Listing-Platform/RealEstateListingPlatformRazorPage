using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize(Roles = "Admin,Lister,Seeker")]
    public class ManualCompleteModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public ManualCompleteModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnPostAsync(Guid transactionId, string paymentReference, string? notes)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                return new JsonResult(new { success = false, message = "Transaction not found" });
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                return new JsonResult(new { success = false, message = "Unauthorized access" });
            }

            var completeDto = new CompleteTransactionDto
            {
                TransactionId = transactionId,
                PaymentReference = paymentReference,
                Notes = notes
            };

            var result = await _paymentService.CompleteTransactionAsync(completeDto);

            if (!result.Success)
            {
                return new JsonResult(new { success = false, message = result.Message });
            }

            return new JsonResult(new { success = true, message = "Payment marked as completed. Awaiting admin approval." });
        }
    }
}
