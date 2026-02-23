using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize]
    public class CheckStatusModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public CheckStatusModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync(Guid transactionId)
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

            // If already completed, return success
            if (transaction.Data.Status == "Completed")
            {
                return new JsonResult(new
                {
                    success = true,
                    status = "Completed",
                    message = "Payment completed successfully!",
                    redirectUrl = Url.Page("/Payment/Success", new { transactionId = transactionId })
                });
            }

            // TODO: Check with PayOS if payment was made
            // For now, return pending status
            return new JsonResult(new
            {
                success = true,
                status = "Pending",
                message = "Payment is still pending. Please complete the payment."
            });
        }
    }
}
