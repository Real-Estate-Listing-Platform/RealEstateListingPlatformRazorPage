using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, message = "Message cannot be empty" });

            var response = await _chatbotService.ChatAsync(request.Message, request.History ?? new List<ChatMessageDto>());
            return Ok(response);
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] ChatbotRequestDto request)
        {
            var listings = await _chatbotService.GetListingRecommendationsAsync(request.Message);
            return Ok(new { success = true, listings });
        }
    }
}
