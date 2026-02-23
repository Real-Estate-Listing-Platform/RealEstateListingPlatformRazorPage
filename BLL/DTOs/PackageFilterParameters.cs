using System;

namespace BLL.DTOs
{
    public class PackageFilterParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search
        public string? SearchTerm { get; set; }

        // Filters
        public string? Status { get; set; }
        public string? PackageType { get; set; }
        public DateTime? PurchasedAfter { get; set; }
        public DateTime? PurchasedBefore { get; set; }

        // Sorting
        public string SortBy { get; set; } = "PurchasedAt";
        public string SortOrder { get; set; } = "desc"; // "asc" or "desc"
    }
}

