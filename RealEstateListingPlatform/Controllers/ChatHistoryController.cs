using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstateListingPlatform.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatHistoryController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetMySessions([FromQuery] ChatHistoryFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.GetUserSessionsPaginatedAsync(userId, filter);
            return Ok(result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.GetSessionAsync(sessionId, userId);
            return Ok(result);
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.CreateSessionAsync(userId, dto);
            return Ok(result);
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.DeleteSessionAsync(sessionId, userId);
            return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/archive")]
        public async Task<IActionResult> ArchiveSession(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.ArchiveSessionAsync(sessionId, userId);
            return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/restore")]
        public async Task<IActionResult> RestoreSession(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.RestoreSessionAsync(sessionId, userId);
            return Ok(result);
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetSessionMessages(Guid sessionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.GetSessionMessagesPaginatedAsync(sessionId, userId, page, pageSize);
            return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageDto dto)
        {
            if (dto.SessionId != sessionId)
                dto.SessionId = sessionId;

            var userId = GetCurrentUserId();
            var result = await _chatService.SendMessageAsync(userId, dto);
            return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/messages/read")]
        public async Task<IActionResult> MarkMessagesAsRead(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.MarkMessagesAsReadAsync(sessionId, userId);
            return Ok(result);
        }

        [HttpPost("messages/{messageId}/feedback")]
        public async Task<IActionResult> AddFeedback(Guid messageId, [FromBody] ChatFeedbackDto dto)
        {
            dto.MessageId = messageId;
            var userId = GetCurrentUserId();
            var result = await _chatService.AddFeedbackAsync(userId, dto);
            return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/summarize")]
        public async Task<IActionResult> SummarizeSession(Guid sessionId)
        {
            var result = await _chatService.SummarizeSessionAsync(sessionId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _chatService.GetChatStatisticsAsync(fromDate, toDate);
            return Ok(result);
        }
    }
}