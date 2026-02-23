using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Payment
{
    [Authorize]
    public class MyTransactionsModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public MyTransactionsModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public List<TransactionDto> Transactions { get; set; } = new();

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetUserTransactionsAsync(userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                Transactions = new List<TransactionDto>();
                return Page();
            }

            Transactions = result.Data ?? new List<TransactionDto>();
            return Page();
        }
    }
}
