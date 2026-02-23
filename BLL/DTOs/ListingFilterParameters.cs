using System;

namespace BLL.DTOs
{
    public class ListingFilterParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search
        public string? SearchTerm { get; set; }

        // Filters
        public string? Status { get; set; }
        public string? TransactionType { get; set; }
        public string? PropertyType { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Sorting
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc"; // "asc" or "desc"
    }
}
