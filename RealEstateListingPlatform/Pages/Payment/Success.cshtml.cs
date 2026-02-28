using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize(Roles = "Admin,Lister,Seeker")]
    public class SuccessModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public SuccessModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid TransactionId { get; set; }

        public TransactionDto? Transaction { get; set; }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(TransactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToPage("/Package/Index");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToPage("/Package/Index");
            }

            Transaction = transaction.Data;
            return Page();
        }
    }
}
