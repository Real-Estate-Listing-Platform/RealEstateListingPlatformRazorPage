using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageService _packageService;
        private readonly IPayOSService _payOSService;

        public CallbackController(
            IPaymentService paymentService,
            IPackageService packageService,
            IPayOSService payOSService)
        {
            _paymentService = paymentService;
            _packageService = packageService;
            _payOSService = payOSService;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> Callback(Webhook webhook)
        {
            Console.WriteLine($"[PayOS Webhook] Received webhook for OrderCode: {webhook.Data.OrderCode}");

            var verifiedData = await _payOSService.VerifyWebhookDataAsync(webhook);

            // Find transaction by order code using PaymentService
            var transactionResult = await _paymentService.GetTransactionByPayOSOrderCodeAsync(verifiedData.OrderCode);
            
            if (!transactionResult.Success || transactionResult.Data == null)
            {
                Console.WriteLine($"[PayOS Webhook] Transaction not found for OrderCode: {verifiedData.OrderCode}");
                return BadRequest("Transaction not found");
            }

            var transaction = transactionResult.Data;

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
            return Ok("ok");
        }
    }
}
