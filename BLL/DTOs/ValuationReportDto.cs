namespace BLL.DTOs
{
    public class ValuationReportDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? ReportName { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal AreaSqm { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? AddressLine { get; set; }
        public string? Notes { get; set; }
        public decimal? EstimatedMinPrice { get; set; }
        public decimal? EstimatedAvgPrice { get; set; }
        public decimal? EstimatedMaxPrice { get; set; }
        public decimal? AvgPricePerSqm { get; set; }
        public int SampleCount { get; set; }
        public bool IsFallbackToCity { get; set; }
        public string MarketInsight { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string DisplayName => !string.IsNullOrWhiteSpace(ReportName)
            ? ReportName
            : $"{MapType(PropertyType)} – {District}, {City} ({AreaSqm:0.##} m²)";

        private static string MapType(string raw) => raw switch
        {
            "Apartment"  => "Căn hộ",
            "House"      => "Nhà phố",
            "Villa"      => "Biệt thự",
            "Land"       => "Đất",
            "Commercial" => "Thương mại",
            _            => raw
        };
    }

    public class SaveReportDto
    {
        public string? ReportName { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal AreaSqm { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? AddressLine { get; set; }
        public string? Notes { get; set; }
        public decimal? EstimatedMinPrice { get; set; }
        public decimal? EstimatedAvgPrice { get; set; }
        public decimal? EstimatedMaxPrice { get; set; }
        public decimal? AvgPricePerSqm { get; set; }
        public int SampleCount { get; set; }
        public bool IsFallbackToCity { get; set; }
        public string MarketInsight { get; set; } = string.Empty;
    }
}
