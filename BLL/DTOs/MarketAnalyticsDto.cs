namespace BLL.DTOs
{
    /// <summary>One data point in the price-trend line chart (per month × district).</summary>
    public class PriceTrendPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        /// <summary>Label for the X-axis, e.g. "01/2025"</summary>
        public string MonthLabel => $"{Month:D2}/{Year}";
        public string District { get; set; } = string.Empty;
        public decimal AvgPricePerSqm { get; set; }
        public int ListingCount { get; set; }
    }

    /// <summary>Current price statistics for a single district (bar chart).</summary>
    public class DistrictPriceStatDto
    {
        public string District { get; set; } = string.Empty;
        public decimal AvgPricePerSqm { get; set; }
        public decimal MinPricePerSqm { get; set; }
        public decimal MaxPricePerSqm { get; set; }
        public int ListingCount { get; set; }
    }

    /// <summary>Listing count breakdown by property type (doughnut chart).</summary>
    public class PropertyTypeCountDto
    {
        public string PropertyType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>Top-level summary numbers shown on the stat cards.</summary>
    public class MarketSummaryDto
    {
        public int TotalListings { get; set; }
        public decimal AvgPricePerSqm { get; set; }
        public string TopDistrict { get; set; } = string.Empty;
        public int TopDistrictCount { get; set; }
        /// <summary>MoM change in avg price/m² (%). Null if insufficient history.</summary>
        public decimal? PriceChangePct { get; set; }
    }

    /// <summary>Full payload returned to the Razor page.</summary>
    public class MarketAnalyticsResultDto
    {
        public MarketSummaryDto Summary { get; set; } = new();
        public List<PriceTrendPointDto> PriceTrend { get; set; } = new();
        public List<DistrictPriceStatDto> DistrictStats { get; set; } = new();
        public List<PropertyTypeCountDto> TypeDistribution { get; set; } = new();

        // Filter echo
        public string City { get; set; } = string.Empty;
        public string? PropertyType { get; set; }
        public string? TransactionType { get; set; }
        public int Months { get; set; }
    }

    // ── RELP-57: additional aggregation DTOs ─────────────────────────────────

    /// <summary>A district with notable price growth in the period.</summary>
    public class HotspotDto
    {
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal CurrentAvgPricePerSqm { get; set; }
        public decimal PreviousAvgPricePerSqm { get; set; }
        /// <summary>Period-over-period growth % (positive = rising, negative = falling).</summary>
        public decimal GrowthPct { get; set; }
        public int ListingCount { get; set; }
    }

    /// <summary>One price bracket bucket for the price-range distribution chart.</summary>
    public class PriceRangeBucketDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>Cross-city comparison of avg price/m².</summary>
    public class CityComparisonDto
    {
        public string City { get; set; } = string.Empty;
        public decimal AvgPricePerSqm { get; set; }
        public decimal MedianPrice { get; set; }
        public int ListingCount { get; set; }
        public decimal? MomChangePct { get; set; }
    }

    /// <summary>Generic envelope for all market API responses.</summary>
    public class MarketApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public T? Data { get; set; }
        public string? Message { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
