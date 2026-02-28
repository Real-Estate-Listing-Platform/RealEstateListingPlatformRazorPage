using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Payment
{
    [AllowAnonymous]
    public class PayOSReturnModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IPayOSService _payOSService;

        public PayOSReturnModel(IPaymentService paymentService, IPayOSService payOSService)
        {
            _paymentService = paymentService;
            _payOSService = payOSService;
        }

        public async Task<IActionResult> OnGetAsync(
            [FromQuery] string code,
            [FromQuery] string id,
            [FromQuery] string? cancel,
            [FromQuery] string? status,
            [FromQuery] long orderCode)
        {
            try
            {
                var transactionResult = await _paymentService.GetTransactionByPayOSOrderCodeAsync(orderCode);

                if (!transactionResult.Success || transactionResult.Data == null)
                {
                    TempData["Error"] = "Transaction not found";
                    return RedirectToPage("/Package/Index");
                }

                var transaction = transactionResult.Data;

                // If the webhook already completed the transaction, go straight to success
                if (string.Equals(transaction.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToPage("/Payment/Success", new { transactionId = transaction.Id });
                }

                if (code == "00" && !string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    var paymentInfo = await _payOSService.GetPaymentInfoAsync(orderCode);

                    if (paymentInfo != null && string.Equals(paymentInfo.Status, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Payment/Success", new { transactionId = transaction.Id });
                    }
                }

                return RedirectToPage("/Payment/Failed", new { transactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing return: {ex.Message}";
                return RedirectToPage("/Package/Index");
            }
        }
    }
}
