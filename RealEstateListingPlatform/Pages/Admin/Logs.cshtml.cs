using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Pages.Admin
{
    [Authorize(Roles = "Admin,Lister,Seeker")]
    public class LogsModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IAuditLogService _auditLogService;

        public LogsModel(IListingService listingService, IAdminDashboardService adminDashboardService, IAuditLogService auditLogService)
        {
            _listingService = listingService;
            _adminDashboardService = adminDashboardService;
            _auditLogService = auditLogService;
        }

        public AdminPortalViewModel ViewModel { get; set; } = new();
        public string Section { get; set; } = "logs";
        public int PendingCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 10) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            ViewModel = await BuildAdminPortalViewModelAsync(page, pageSize);
            Section = "logs";
            PendingCount = ViewModel.PendingListings.Count;

            // Set ViewData for layout
            ViewData["Section"] = Section;
            ViewData["PendingCount"] = PendingCount;

            return Page();
        }

        private async Task<AdminPortalViewModel> BuildAdminPortalViewModelAsync(int page = 1, int pageSize = 20)
        {
            var pendingListings = await GetPendingListingApprovalsAsync();
            var (auditLogs, totalCount, totalPages) = await GetAuditLogsAsync(page, pageSize);

            return new AdminPortalViewModel
            {
                PendingListings = pendingListings,
                Stats = BuildAdminStats(),
                Logs = auditLogs,
                Users = BuildAdminUsers(),
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalLogs = totalCount
            };
        }

        private async Task<List<ListingApprovalViewModel>> GetPendingListingApprovalsAsync()
        {
            var listings = await _listingService.GetPendingListingsAsync();
            if (listings == null)
                return new List<ListingApprovalViewModel>();

            return listings.Select(l =>
            {
                var imageUrls = l.ListingMedia?
                    .Where(m => m.MediaType == "image" && !string.IsNullOrWhiteSpace(m.Url))
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Id)
                    .Select(m => m.Url!)
                    .ToList() ?? new List<string>();

                return new ListingApprovalViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Price = l.Price,
                    Description = l.Description ?? "N/A",
                    TransactionType = l.TransactionType ?? "N/A",
                    PropertyType = l.PropertyType ?? "N/A",
                    Area = l.Area ?? "N/A",
                    FurnitureStatus = l.FurnitureStatus ?? "N/A",
                    Direction = l.Direction ?? "N/A",
                    Bedrooms = l.Bedrooms,
                    Bathrooms = l.Bathrooms,
                    Floors = l.Floors,
                    LegalStatus = l.LegalStatus ?? "N/A",
                    Address = $"{l.StreetName},{l.Ward},{l.District}, {l.City}",
                    CreatedAt = l.CreatedAt ?? DateTime.Now,
                    ListerName = l.ListerName ?? "Unknown User",
                    ImageUrls = imageUrls,
                    ImageUrl = imageUrls.FirstOrDefault() ?? string.Empty,
                    IsUpdate = l.PendingSnapshotId.HasValue
                };
            }).ToList();
        }

        private static List<AdminStatCardViewModel> BuildAdminStats()
        {
            return new List<AdminStatCardViewModel>
            {
                new AdminStatCardViewModel
                {
                    Title = "Listings Pending",
                    Value = "24",
                    Delta = "+6 this week",
                    Icon = "bi bi-hourglass-split",
                    Tone = "warning"
                },
                new AdminStatCardViewModel
                {
                    Title = "New Users",
                    Value = "128",
                    Delta = "+18 today",
                    Icon = "bi bi-people",
                    Tone = "success"
                },
                new AdminStatCardViewModel
                {
                    Title = "Revenue",
                    Value = "92.4M VND",
                    Delta = "+12.2% MoM",
                    Icon = "bi bi-coin",
                    Tone = "primary"
                },
                new AdminStatCardViewModel
                {
                    Title = "Reports",
                    Value = "7",
                    Delta = "2 urgent",
                    Icon = "bi bi-flag",
                    Tone = "danger"
                }
            };
        }

        private async Task<(List<AdminLogEntryViewModel>, int totalCount, int totalPages)> GetAuditLogsAsync(int page, int pageSize)
        {
            var (auditLogs, totalCount) = await _auditLogService.GetAuditLogsPaginatedAsync(page, pageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var logs = auditLogs.Select(log => new AdminLogEntryViewModel
            {
                Title = FormatActionTitle(log.ActionType),
                Detail = FormatActionDetail(log),
                OccurredAt = FormatTimeAgo(log.CreatedAt),
                Status = DetermineStatus(log.ActionType),
                Category = DetermineCategory(log.TargetType ?? "System")
            }).ToList();

            return (logs, totalCount, totalPages);
        }

        private string FormatActionTitle(string actionType)
        {
            return actionType switch
            {
                "ListingCreated" => "New listing created",
                "ListingUpdated" => "Listing updated",
                "ListingDeleted" => "Listing deleted",
                "ListingSubmittedForReview" => "Listing submitted for review",
                "ListingApproved" => "Listing approved",
                "ListingRejected" => "Listing rejected",
                "ListingEditedPendingApproval" => "Listing edited - pending approval",
                "UserRegistered" => "New user registered",
                "UserLogin" => "User logged in",
                "PackagePurchased" => "Package purchased",
                "PackageRefunded" => "Package refunded",
                "LeadCreated" => "New lead generated",
                _ => actionType
            };
        }

        private string FormatActionDetail(BLL.DTOs.AuditLogDto log)
        {
            var actor = !string.IsNullOrEmpty(log.ActorUserName) ? log.ActorUserName : "System";
            var target = log.TargetType ?? "Unknown";
            var ipInfo = !string.IsNullOrEmpty(log.IpAddress) ? $" from {log.IpAddress}" : "";

            return $"{actor} performed action on {target}{ipInfo}";
        }

        private string FormatTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";

            return dateTime.ToString("MMM dd, yyyy HH:mm");
        }

        private string DetermineStatus(string actionType)
        {
            return actionType switch
            {
                "ListingApproved" => "Completed",
                "ListingRejected" => "Rejected",
                "ListingSubmittedForReview" => "Pending Review",
                "ListingEditedPendingApproval" => "Pending Review",
                "PackagePurchased" => "Successful",
                "ListingDeleted" => "Completed",
                "UserLogin" => "Successful",
                _ => "Completed"
            };
        }

        private string DetermineCategory(string targetType)
        {
            return targetType switch
            {
                "Listing" => "Listing",
                "User" => "User",
                "Package" => "Payments",
                "Lead" => "Lead",
                "Report" => "Support",
                _ => "System"
            };
        }

        private static List<AdminUserRowViewModel> BuildAdminUsers()
        {
            return new List<AdminUserRowViewModel>
            {
                new AdminUserRowViewModel
                {
                    Name = "Nguyen Minh Anh",
                    Email = "minh.anh@estately.vn",
                    Role = "Lister",
                    Status = "Active",
                    LastActive = "5 minutes ago",
                    Plan = "Boost 30 Days"
                },
                new AdminUserRowViewModel
                {
                    Name = "Thanh Phuong",
                    Email = "thanh.phuong@estately.vn",
                    Role = "Agency",
                    Status = "Active",
                    LastActive = "Today, 08:05",
                    Plan = "Photo Pack +20"
                },
                new AdminUserRowViewModel
                {
                    Name = "Hong Linh",
                    Email = "hong.linh@estately.vn",
                    Role = "Lister",
                    Status = "Suspended",
                    LastActive = "Jan 20, 2026",
                    Plan = "Additional Listing"
                },
                new AdminUserRowViewModel
                {
                    Name = "Tuan Kiet",
                    Email = "tuan.kiet@estately.vn",
                    Role = "Moderator",
                    Status = "Active",
                    LastActive = "Today, 11:40",
                    Plan = "Admin"
                }
            };
        }
    }
}
