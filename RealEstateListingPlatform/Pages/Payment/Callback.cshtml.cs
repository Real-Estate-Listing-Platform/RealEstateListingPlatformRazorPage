using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayOS.Models.Webhooks;

namespace RealEstateListingPlatform.Pages.Payment
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class CallbackModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageService _packageService;
        private readonly IPayOSService _payOSService;

        public CallbackModel(
            IPaymentService paymentService,
            IPackageService packageService,
            IPayOSService payOSService)
        {
            _paymentService = paymentService;
            _packageService = packageService;
            _payOSService = payOSService;
        }

        public IActionResult OnGet()
        {
            return new JsonResult(new
            {
                message = "Webhook endpoint is accessible",
                timestamp = DateTime.UtcNow,
                method = "GET",
                note = "POST requests are used for actual webhook callbacks"
            });
        }

        public async Task<IActionResult> OnPostAsync([FromBody] Webhook webhook)
        {
            try
            {
                Console.WriteLine($"[PayOS Webhook] Received webhook for OrderCode: {webhook.Data.OrderCode}");

                var verifiedData = await _payOSService.VerifyWebhookDataAsync(webhook);

                if (verifiedData == null)
                {
                    Console.WriteLine($"[PayOS Webhook] Webhook verification failed for OrderCode: {webhook.Data.OrderCode}");
                }

                // Find transaction by order code using PaymentService
                var transactionResult = await _paymentService.GetTransactionByPayOSOrderCodeAsync(verifiedData.OrderCode);

                if (!transactionResult.Success || transactionResult.Data == null)
                {
                    Console.WriteLine($"[PayOS Webhook] Transaction not found for OrderCode: {verifiedData.OrderCode}");
                }

                var transaction = transactionResult.Data;
                if (transaction == null) return new OkObjectResult("ok");
                // Process based on status
                if (verifiedData.Code == "00")
                {
                    // Update PayOS transaction reference
                    await _paymentService.UpdateTransactionPayOSReferenceAsync(
                        transaction.Id,
                        verifiedData.Reference ?? verifiedData.OrderCode.ToString()
                    );
                    var completeDto = new CompleteTransactionDto
                    {
                        TransactionId = transaction.Id,
                        PaymentReference = verifiedData.Reference ?? verifiedData.OrderCode.ToString(),
                        Notes = $"PayOS webhook - Payment successful"
                    };

                    var completeResult = await _paymentService.CompleteTransactionAsync(completeDto);

                    if (completeResult.Success)
                    {
                        await _packageService.ActivateUserPackageAsync(transaction.Id);
                    }
                }
                else
                {
                    await _paymentService.FailTransactionAsync(
                        transaction.Id,
                        $"PayOS payment failed/cancelled with code: {verifiedData.Code}"
                    );
                }
            } catch (Exception e) { };
            

            return new OkObjectResult("ok");
        }
    }
}
