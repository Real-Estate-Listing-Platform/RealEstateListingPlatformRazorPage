namespace BLL.DTOs
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = "user"; // "user" or "model"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ChatbotRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto> History { get; set; } = new();
    }

    public class ChatbotResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ListingDto> SuggestedListings { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? Error { get; set; }
    }
}
