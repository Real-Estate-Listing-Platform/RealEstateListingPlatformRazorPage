using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using System.Security.Claims;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class CustomersModel : PageModel
    {
        private readonly ILeadService _leadService;

        public CustomersModel(ILeadService leadService)
        {
            _leadService = leadService;
        }

        public void OnGet()
        {
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims");
            return Guid.Parse(userIdClaim);
        }

        // GET: /Lister/Customers?handler=Statistics
        public async Task<IActionResult> OnGetStatisticsAsync()
        {
            var userId = GetCurrentUserId();
            var result = await _leadService.GetLeadStatisticsAsync(userId);

            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Success ? new
                {
                    totalLeads = result.Data?.TotalLeads ?? 0,
                    newLeads = result.Data?.NewLeads ?? 0,
                    contactedLeads = result.Data?.ContactedLeads ?? 0,
                    closedLeads = result.Data?.ClosedLeads ?? 0
                } : null
            });
        }

        // GET: /Lister/Customers?handler=MyLeadsAsLister
        public async Task<IActionResult> OnGetMyLeadsAsListerAsync()
        {
            var userId = GetCurrentUserId();
            var result = await _leadService.GetMyLeadsAsListerAsync(userId);

            if (!result.Success || result.Data == null)
            {
                return new JsonResult(new { success = false, message = result.Message });
            }

            var leads = result.Data.Select(lead => new
            {
                id = lead.Id.ToString(),
                seekerName = lead.Seeker?.DisplayName ?? "Unknown",
                seekerEmail = lead.Seeker?.Email ?? "",
                seekerPhone = lead.Seeker?.Phone,
                listingTitle = lead.Listing?.Title ?? "Unknown Listing",
                listingAddress = $"{lead.Listing?.HouseNumber}, {lead.Listing?.StreetName}, {lead.Listing?.Ward}, {lead.Listing?.District}, {lead.Listing?.City}",
                listingPrice = lead.Listing?.Price ?? 0,
                message = lead.Message,
                status = lead.Status,
                appointmentDate = lead.AppointmentDate,
                listerNote = lead.ListerNote,
                createdAt = lead.CreatedAt,
                timeAgo = GetTimeAgo(lead.CreatedAt)
            }).ToList();

            return new JsonResult(new { success = true, data = leads });
        }

        // POST: /Lister/Customers?handler=UpdateStatus
        public async Task<IActionResult> OnPostUpdateStatusAsync([FromBody] UpdateLeadStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.LeadId))
            {
                return new JsonResult(new { success = false, message = "Invalid request" });
            }

            var userId = GetCurrentUserId();
            var leadId = Guid.Parse(request.LeadId);
            var result = await _leadService.UpdateLeadStatusAsync(leadId, userId, request.Status, request.ListerNote);

            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        private static string GetTimeAgo(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Unknown";

            var timeSpan = DateTime.UtcNow - dateTime.Value;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

            return dateTime.Value.ToString("MMM dd, yyyy");
        }
    }

    public class UpdateLeadStatusRequest
    {
        public string LeadId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ListerNote { get; set; }
    }
}
