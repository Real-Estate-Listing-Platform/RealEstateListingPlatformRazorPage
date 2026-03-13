using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private const string SessionKey = "ChatHistory";
        private const int MaxHistoryMessages = 50;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var history = GetSessionHistory();
            return Ok(new { success = true, history });
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, message = "Message cannot be empty" });

            var history = GetSessionHistory();
            var response = await _chatbotService.ChatAsync(request.Message, history);

            if (response.Success)
            {
                history.Add(new ChatMessageDto { Role = "user", Content = request.Message, Timestamp = DateTime.UtcNow });
                history.Add(new ChatMessageDto { Role = "model", Content = response.Message, Timestamp = DateTime.UtcNow });

                if (history.Count > MaxHistoryMessages)
                    history = history.Skip(history.Count - MaxHistoryMessages).ToList();

                SaveSessionHistory(history);
            }

            return Ok(response);
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] ChatbotRequestDto request)
        {
            var listings = await _chatbotService.GetListingRecommendationsAsync(request.Message);
            return Ok(new { success = true, listings });
        }

        [HttpDelete("history")]
        public IActionResult ClearHistory()
        {
            HttpContext.Session.Remove(SessionKey);
            return Ok(new { success = true, message = "Chat history cleared" });
        }

        private List<ChatMessageDto> GetSessionHistory()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new List<ChatMessageDto>();
            return JsonSerializer.Deserialize<List<ChatMessageDto>>(json) ?? new List<ChatMessageDto>();
        }

        private void SaveSessionHistory(List<ChatMessageDto> history)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(history));
        }
    }
}
