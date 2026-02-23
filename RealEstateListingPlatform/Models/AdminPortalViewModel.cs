using System;
using System.Collections.Generic;

namespace RealEstateListingPlatform.Models
{
    public class AdminPortalViewModel
    {
        public IReadOnlyList<AdminStatCardViewModel> Stats { get; set; } = new List<AdminStatCardViewModel>();
        public IReadOnlyList<ListingApprovalViewModel> PendingListings { get; set; } = new List<ListingApprovalViewModel>();
        public IReadOnlyList<AdminLogEntryViewModel> Logs { get; set; } = new List<AdminLogEntryViewModel>();
        public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = new List<AdminUserRowViewModel>();
        
        // Pagination properties for Logs
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalLogs { get; set; } = 0;
    }

    public class AdminStatCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Delta { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
    }

    public class AdminLogEntryViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string OccurredAt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class AdminUserRowViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastActive { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
    }
}
