namespace BLL.DTOs
{
    public class ListingMediaDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public string MediaType { get; set; } = null!; // "image" or "video"
        public string Url { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
