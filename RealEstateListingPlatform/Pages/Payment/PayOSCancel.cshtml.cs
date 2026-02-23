using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;

namespace RealEstateListingPlatform.Pages.Payment
{
    [AllowAnonymous]
    public class PayOSCancelModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public PayOSCancelModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] long orderCode)
        {
            try
            {
                var transactionResult = await _paymentService.GetTransactionByPayOSOrderCodeAsync(orderCode);

                if (transactionResult.Success && transactionResult.Data != null)
                {
                    await _paymentService.FailTransactionAsync(transactionResult.Data.Id, "Payment cancelled by user");
                    return RedirectToPage("/Payment/Failed", new { transactionId = transactionResult.Data.Id });
                }

                TempData["Error"] = "Transaction not found";
                return RedirectToPage("/Package/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing cancellation: {ex.Message}";
                return RedirectToPage("/Package/Index");
            }
        }
    }
}
