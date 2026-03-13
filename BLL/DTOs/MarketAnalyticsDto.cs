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
}
