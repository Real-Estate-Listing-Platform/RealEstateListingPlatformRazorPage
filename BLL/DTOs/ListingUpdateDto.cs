namespace BLL.DTOs
{
    public class ListingUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? TransactionType { get; set; }
        public string? PropertyType { get; set; }
        public decimal? Price { get; set; }
        public string? StreetName { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? HouseNumber { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Floors { get; set; }
        public string? LegalStatus { get; set; }
        public string? FurnitureStatus { get; set; }
        public string? Direction { get; set; }
    }
}
