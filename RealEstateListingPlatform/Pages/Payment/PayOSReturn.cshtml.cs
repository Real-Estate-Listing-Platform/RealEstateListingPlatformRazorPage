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

                if (code == "00" && status != "CANCELLED")
                {
                    var paymentInfo = await _payOSService.GetPaymentInfoAsync(orderCode);

                    if (paymentInfo != null && paymentInfo.Status == "PAID")
                    {
                        return RedirectToPage("/Payment/Success", new { transactionId = transactionResult.Data.Id });
                    }
                }

                return RedirectToPage("/Payment/Failed", new { transactionId = transactionResult.Data.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing return: {ex.Message}";
                return RedirectToPage("/Package/Index");
            }
        }
    }
}
