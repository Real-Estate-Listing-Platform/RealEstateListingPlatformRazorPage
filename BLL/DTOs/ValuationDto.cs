namespace BLL.DTOs
{
    public class ValuationResultDto
    {
        public bool HasData { get; set; }

        // Input echo
        public string PropertyType { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal AreaSqm { get; set; }

        // Price-per-sqm statistics (computed from comparable listings)
        public decimal? AvgPricePerSqm { get; set; }
        public decimal? MinPricePerSqm { get; set; }
        public decimal? MaxPricePerSqm { get; set; }

        // Estimated total price range for the requested area
        public decimal? EstimatedMinPrice { get; set; }
        public decimal? EstimatedAvgPrice { get; set; }
        public decimal? EstimatedMaxPrice { get; set; }

        // Sample info
        public int SampleCount { get; set; }

        // true when district had < 3 results and we broadened to city-level
        public bool IsFallbackToCity { get; set; }

        public string MarketInsight { get; set; } = string.Empty;

        public List<ComparableListingDto> ComparableListings { get; set; } = new();
    }

    public class ComparableListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? AreaSqm { get; set; }
        public decimal? PricePerSqm { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
