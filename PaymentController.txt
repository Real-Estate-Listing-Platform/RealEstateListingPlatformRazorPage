using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;
using PayOS.Models.Webhooks;

namespace RealEstateListingPlatform.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageService _packageService;
        private readonly IPayOSService _payOSService;

        public PaymentController(
            IPaymentService paymentService, 
            IPackageService packageService,
            IPayOSService payOSService)
        {
            _paymentService = paymentService;
            _packageService = packageService;
            _payOSService = payOSService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        // GET: Payment/Process - Payment gateway redirect
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Process(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            // Only pending transactions can be processed
            if (transaction.Data.Status != "Pending")
            {
                if (transaction.Data.Status == "Completed")
                {
                    return RedirectToAction(nameof(Success), new { transactionId = transactionId });
                }
                TempData["Error"] = "This transaction cannot be processed";
                return RedirectToAction("Index", "Package");
            }

            // Get or create PayOS payment link
            var paymentResult = await _paymentService.InitiatePaymentAsync(
                transactionId, 
                transaction.Data.PaymentMethod ?? "PAYOS"
            );

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.Data))
            {
                TempData["Error"] = paymentResult.Message ?? "Failed to create payment link";
                return RedirectToAction("Index", "Package");
            }

            ViewBag.CheckoutUrl = paymentResult.Data;
            ViewBag.Transaction = transaction.Data;
            return View(transaction.Data);
        }

        // GET: Payment/CheckStatus - Check payment status via AJAX
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckStatus(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            // If already completed, return success
            if (transaction.Data.Status == "Completed")
            {
                return Json(new { 
                    success = true, 
                    status = "Completed",
                    message = "Payment completed successfully!",
                    redirectUrl = Url.Action("Success", new { transactionId = transactionId })
                });
            }

            // TODO: Check with PayOS if payment was made
            // For now, return pending status
            return Json(new { 
                success = true, 
                status = "Pending",
                message = "Payment is still pending. Please complete the payment."
            });
        }

        // GET: Payment/Callback/Test - Test endpoint to verify webhook is accessible
        [HttpGet("/Payment/Callback")]
        [AllowAnonymous]
        public IActionResult TestWebhook()
        {
            return Ok(new { 
                message = "Webhook endpoint is accessible", 
                timestamp = DateTime.UtcNow,
                method = "GET",
                note = "POST requests are used for actual webhook callbacks"
            });
        }

        // POST: Payment/Callback - Payment gateway callback (webhook)
        

        // GET: Payment/PayOSReturn - PayOS return URL
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
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
                    return RedirectToAction("Index", "Package");
                }

                if (code == "00" && status != "CANCELLED")
                {
                    var paymentInfo = await _payOSService.GetPaymentInfoAsync(orderCode);
                    
                    if (paymentInfo != null && paymentInfo.Status == "PAID")
                    {
                        return RedirectToAction(nameof(Success), new { transactionId = transactionResult.Data.Id });
                    }
                }

                return RedirectToAction(nameof(Failed), new { transactionId = transactionResult.Data.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing return: {ex.Message}";
                return RedirectToAction("Index", "Package");
            }
        }

        // GET: Payment/PayOSCancel - PayOS cancel URL
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSCancel([FromQuery] long orderCode)
        {
            try
            {
                var transactionResult = await _paymentService.GetTransactionByPayOSOrderCodeAsync(orderCode);
                
                if (transactionResult.Success && transactionResult.Data != null)
                {
                    await _paymentService.FailTransactionAsync(transactionResult.Data.Id, "Payment cancelled by user");
                    return RedirectToAction(nameof(Failed), new { transactionId = transactionResult.Data.Id });
                }

                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing cancellation: {ex.Message}";
                return RedirectToAction("Index", "Package");
            }
        }

        // GET: Payment/Success - Payment success page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Success(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            ViewBag.Transaction = transaction.Data;
            return View();
        }

        // GET: Payment/Failed - Payment failed page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Failed(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            ViewBag.Transaction = transaction.Data;
            return View();
        }

        // POST: Payment/ManualComplete - Manually complete payment (for bank transfer, admin approval)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualComplete(Guid transactionId, string paymentReference, string? notes)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized access" });
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
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "Payment marked as completed. Awaiting admin approval." });
        }

        // GET: Payment/MyTransactions - User's transaction history
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTransactions()
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetUserTransactionsAsync(userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<TransactionDto>());
            }

            return View(result.Data);
        }

        // POST: Payment/RetryPayment - Retry failed payment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryPayment(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Only allow retry for Pending or Failed transactions
            if (transaction.Data.Status != "Pending" && transaction.Data.Status != "Failed")
            {
                TempData["Error"] = "This transaction cannot be retried";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Create new PayOS payment link
            var paymentResult = await _paymentService.InitiatePaymentAsync(
                transactionId, 
                transaction.Data.PaymentMethod ?? "PAYOS"
            );

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.Data))
            {
                TempData["Error"] = paymentResult.Message ?? "Failed to create payment link";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Redirect to PayOS payment page
            return Redirect(paymentResult.Data);
        }
    }
}
