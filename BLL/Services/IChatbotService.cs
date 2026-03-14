using BLL.DTOs;

namespace BLL.Services
{
    public interface IChatbotService
    {
        Task<ChatbotResponseDto> ChatAsync(string userMessage, List<ChatMessageDto> history);
        Task<List<ListingDto>> GetListingRecommendationsAsync(string userMessage);
    }
}
