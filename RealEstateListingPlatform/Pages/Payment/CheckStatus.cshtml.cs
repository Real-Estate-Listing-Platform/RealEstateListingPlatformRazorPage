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
        private readonly IPayOSService _payOSService;

        public CheckStatusModel(IPaymentService paymentService, IPayOSService payOSService)
        {
            _paymentService = paymentService;
            _payOSService = payOSService;
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

            // Check with PayOS if payment was made
            if (transaction.Data.PayOSOrderCode.HasValue)
            {
                try
                {
                    var orderCode = transaction.Data.PayOSOrderCode.Value;
                    var paymentInfo = await _payOSService.GetPaymentInfoAsync(orderCode);
                    
                    if (paymentInfo != null)
                    {
                        // Update transaction status based on PayOS response
                        if (paymentInfo.Status == "PAID" && transaction.Data.Status != "Completed")
                        {
                            // Update transaction to completed
                            var completeDto = new BLL.DTOs.CompleteTransactionDto
                            {
                                TransactionId = transactionId,
                                PaymentReference = paymentInfo.TransactionReference ?? orderCode.ToString(),
                                Notes = "Payment completed via PayOS"
                            };
                            await _paymentService.CompleteTransactionAsync(completeDto);
                            
                            return new JsonResult(new
                            {
                                success = true,
                                status = "Completed",
                                message = "Payment completed successfully!",
                                redirectUrl = Url.Page("/Payment/Success", new { transactionId = transactionId })
                            });
                        }
                        else if (paymentInfo.Status == "CANCELLED" || paymentInfo.Status == "EXPIRED")
                        {
                            return new JsonResult(new
                            {
                                success = false,
                                status = paymentInfo.Status,
                                message = "Payment was cancelled or expired. Please try again.",
                                redirectUrl = Url.Page("/Payment/Failed", new { transactionId = transactionId })
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking PayOS payment status: {ex.Message}");
                }
            }

            // Return pending status if payment not yet completed
            return new JsonResult(new
            {
                success = true,
                status = "Pending",
                message = "Payment is still pending. Please complete the payment."
            });
        }
    }
}
