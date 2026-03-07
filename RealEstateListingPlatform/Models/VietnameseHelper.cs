using System.Collections.Generic;

namespace RealEstateListingPlatform.Models
{
    /// <summary>
    /// Helper class to convert English data values stored in DB to Vietnamese display text.
    /// </summary>
    public static class VietnameseHelper
    {
        private static readonly Dictionary<string, string> TransactionTypes = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Sell", "Bán" },
            { "Rent", "Cho thuê" },
            { "Sale", "Bán" }
        };

        private static readonly Dictionary<string, string> PropertyTypes = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Apartment", "Căn hộ" },
            { "House", "Nhà phố" },
            { "Townhouse", "Nhà phố" },
            { "Villa", "Biệt thự" },
            { "Land", "Đất" },
            { "Office", "Văn phòng" },
            { "Commercial", "Thương mại" },
            { "Room", "Phòng trọ" },
            { "Penthouse", "Penthouse" }
        };

        private static readonly Dictionary<string, string> Statuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Published", "Đã đăng" },
            { "Draft", "Bản nháp" },
            { "PendingReview", "Chờ duyệt" },
            { "Rejected", "Từ chối" },
            { "Expired", "Hết hạn" }
        };

        private static readonly Dictionary<string, string> LegalStatuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "RedBook", "Sổ đỏ" },
            { "PinkBook", "Sổ hồng" },
            { "SaleContract", "Hợp đồng mua bán" },
            { "Waiting", "Đang chờ cấp sổ" }
        };

        private static readonly Dictionary<string, string> FurnitureStatuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "FullyFurnished", "Đầy đủ nội thất" },
            { "PartiallyFurnished", "Nội thất cơ bản" },
            { "Unfurnished", "Không nội thất" }
        };

        private static readonly Dictionary<string, string> Directions = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "North", "Bắc" },
            { "South", "Nam" },
            { "East", "Đông" },
            { "West", "Tây" },
            { "Northeast", "Đông Bắc" },
            { "Northwest", "Tây Bắc" },
            { "Southeast", "Đông Nam" },
            { "Southwest", "Tây Nam" }
        };

        private static readonly Dictionary<string, string> LeadStatuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "New", "Mới" },
            { "Contacted", "Đã liên hệ" },
            { "Closed", "Đã đóng" }
        };

        private static readonly Dictionary<string, string> TransactionStatuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Completed", "Hoàn thành" },
            { "Pending", "Đang chờ" },
            { "Failed", "Thất bại" },
            { "Refunded", "Hoàn tiền" }
        };

        public static string TransactionType(string? value) =>
            value != null && TransactionTypes.TryGetValue(value, out var result) ? result : value ?? "";

        public static string PropertyType(string? value) =>
            value != null && PropertyTypes.TryGetValue(value, out var result) ? result : value ?? "";

        public static string Status(string? value) =>
            value != null && Statuses.TryGetValue(value, out var result) ? result : value ?? "";

        public static string LegalStatus(string? value) =>
            value != null && LegalStatuses.TryGetValue(value, out var result) ? result : value ?? "";

        public static string FurnitureStatus(string? value) =>
            value != null && FurnitureStatuses.TryGetValue(value, out var result) ? result : value ?? "";

        public static string Direction(string? value) =>
            value != null && Directions.TryGetValue(value, out var result) ? result : value ?? "";

        public static string LeadStatus(string? value) =>
            value != null && LeadStatuses.TryGetValue(value, out var result) ? result : value ?? "";

        public static string TransactionStatus(string? value) =>
            value != null && TransactionStatuses.TryGetValue(value, out var result) ? result : value ?? "";

        /// <summary>
        /// Format "For Sale" / "For Rent" display label from transaction type
        /// </summary>
        public static string TransactionTypeLabel(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Equals("Sell", System.StringComparison.OrdinalIgnoreCase) ? "Đang bán" :
                   value.Equals("Rent", System.StringComparison.OrdinalIgnoreCase) ? "Cho thuê" : value;
        }
    }
}
