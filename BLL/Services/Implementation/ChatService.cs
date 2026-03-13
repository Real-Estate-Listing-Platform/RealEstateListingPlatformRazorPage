using BLL.DTOs;
using BLL.Hubs;
using DAL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class ChatService : IChatService
    {
        private readonly IChatSessionRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IHubContext<DashboardHub, IDashboardClient> _dashboardHub;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;

        public ChatService(
            IChatSessionRepository chatRepository,
            IUserRepository userRepository,
            IListingRepository listingRepository,
            IHubContext<DashboardHub, IDashboardClient> dashboardHub,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _listingRepository = listingRepository;
            _dashboardHub = dashboardHub;
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
        }

        // Session management
        public async Task<ServiceResult<ChatSessionDto>> CreateSessionAsync(Guid userId, CreateChatSessionDto dto)
        {
            try
            {
                var user = await _userRepository.GetUserById(userId);
                if (user == null)
                    return ServiceResult<ChatSessionDto>.FailureResult("User not found");

                var session = new ChatSession
                {
                    UserId = userId,
                    Title = dto.Title,
                    StartedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                var created = await _chatRepository.CreateSessionAsync(session);

                // Add initial message if provided
                if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
                {
                    var userMessage = new ChatMessage
                    {
                        SessionId = created.Id,
                        Sender = "User",
                        Message = dto.InitialMessage,
                        MessageType = "Text"
                    };
                    await _chatRepository.AddMessageAsync(userMessage);

                    // Generate bot response
                    var botResponse = await GenerateBotResponseInternal(dto.InitialMessage, created.Id);
                    if (!string.IsNullOrWhiteSpace(botResponse))
                    {
                        var botMessage = new ChatMessage
                        {
                            SessionId = created.Id,
                            Sender = "Bot",
                            Message = botResponse,
                            MessageType = "Text"
                        };
                        await _chatRepository.AddMessageAsync(botMessage);
                    }

                    // Update session title if empty (use first few words of user message)
                    if (string.IsNullOrWhiteSpace(created.Title) && !string.IsNullOrWhiteSpace(dto.InitialMessage))
                    {
                        var words = dto.InitialMessage.Split(' ').Take(5);
                        created.Title = string.Join(" ", words) + (words.Count() >= 5 ? "..." : "");
                        if (created.Title.Length > 100)
                            created.Title = created.Title[..100];
                        await _chatRepository.UpdateSessionAsync(created);
                    }
                }

                var sessionDto = await MapToDto(created);
                return ServiceResult<ChatSessionDto>.SuccessResult(sessionDto, "Chat session created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatSessionDto>.FailureResult($"Failed to create chat session: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatSessionDto>> GetSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<ChatSessionDto>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<ChatSessionDto>.FailureResult("You do not have permission to view this session");

                var dto = await MapToDto(session);
                return ServiceResult<ChatSessionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatSessionDto>.FailureResult($"Failed to get chat session: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatSessionSummaryDto>>> GetUserSessionsAsync(Guid userId, bool includeArchived = false)
        {
            try
            {
                var sessions = await _chatRepository.GetUserSessionsAsync(userId, includeArchived);
                var dtos = sessions.Select(s => new ChatSessionSummaryDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Summary = s.Summary,
                    StartedAt = s.StartedAt,
                    LastMessageAt = s.LastMessageAt,
                    MessageCount = s.MessageCount,
                    Preview = s.Messages?.FirstOrDefault()?.Message ?? "No messages"
                }).ToList();

                return ServiceResult<List<ChatSessionSummaryDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatSessionSummaryDto>>.FailureResult($"Failed to get user sessions: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<ChatSessionSummaryDto>>> GetUserSessionsPaginatedAsync(
            Guid userId, ChatHistoryFilterDto filter)
        {
            try
            {
                var sessions = await _chatRepository.GetUserSessionsPaginatedAsync(
                    userId, filter.PageNumber, filter.PageSize, filter.SearchTerm, filter.Status);

                var totalCount = await _chatRepository.GetUserSessionsCountAsync(userId, filter.SearchTerm, filter.Status);

                var dtos = sessions.Select(s => new ChatSessionSummaryDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Summary = s.Summary,
                    StartedAt = s.StartedAt,
                    LastMessageAt = s.LastMessageAt,
                    MessageCount = s.MessageCount
                }).ToList();

                var result = new PaginatedResult<ChatSessionSummaryDto>
                {
                    Items = dtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };

                return ServiceResult<PaginatedResult<ChatSessionSummaryDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<ChatSessionSummaryDto>>.FailureResult($"Failed to get user sessions: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ArchiveSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<bool>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<bool>.FailureResult("You do not have permission to archive this session");

                var success = await _chatRepository.ArchiveSessionAsync(sessionId);
                return success
                    ? ServiceResult<bool>.SuccessResult(true, "Session archived successfully")
                    : ServiceResult<bool>.FailureResult("Failed to archive session");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to archive session: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> RestoreSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<bool>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<bool>.FailureResult("You do not have permission to restore this session");

                var success = await _chatRepository.RestoreSessionAsync(sessionId);
                return success
                    ? ServiceResult<bool>.SuccessResult(true, "Session restored successfully")
                    : ServiceResult<bool>.FailureResult("Failed to restore session");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to restore session: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<bool>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<bool>.FailureResult("You do not have permission to delete this session");

                var success = await _chatRepository.DeleteSessionAsync(sessionId);
                return success
                    ? ServiceResult<bool>.SuccessResult(true, "Session deleted successfully")
                    : ServiceResult<bool>.FailureResult("Failed to delete session");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to delete session: {ex.Message}");
            }
        }

        // Message handling
        public async Task<ServiceResult<ChatMessageDto>> SendMessageAsync(Guid userId, SendMessageDto dto)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(dto.SessionId);
                if (session == null)
                    return ServiceResult<ChatMessageDto>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<ChatMessageDto>.FailureResult("You do not have permission to send messages to this session");

                if (session.Status != "Active")
                    return ServiceResult<ChatMessageDto>.FailureResult("Cannot send messages to an archived session");

                // Save user message
                var userMessage = new ChatMessage
                {
                    SessionId = dto.SessionId,
                    Sender = "User",
                    Message = dto.Message,
                    MessageType = dto.MessageType ?? "Text"
                };
                var savedUserMessage = await _chatRepository.AddMessageAsync(userMessage);

                // Generate bot response
                var botResponse = await GenerateBotResponseInternal(dto.Message, dto.SessionId, session.ContextData);
                var botMessage = new ChatMessage
                {
                    SessionId = dto.SessionId,
                    Sender = "Bot",
                    Message = botResponse,
                    MessageType = "Text"
                };
                var savedBotMessage = await _chatRepository.AddMessageAsync(botMessage);

                // Update session context if needed
                await UpdateSessionContextAsync(dto.SessionId, dto.Message, botResponse);

                // Notify via SignalR
                await NotifyNewMessageAsync(dto.SessionId, savedBotMessage);

                // Return the bot message (user can see their own message immediately)
                return ServiceResult<ChatMessageDto>.SuccessResult(MapToMessageDto(savedBotMessage), "Message sent successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"Failed to send message: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatMessageDto>>> GetSessionMessagesAsync(Guid sessionId, Guid userId, int limit = 50)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<List<ChatMessageDto>>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<List<ChatMessageDto>>.FailureResult("You do not have permission to view this session");

                var messages = await _chatRepository.GetSessionMessagesAsync(sessionId, limit);
                var dtos = messages.Select(MapToMessageDto).ToList();

                return ServiceResult<List<ChatMessageDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatMessageDto>>.FailureResult($"Failed to get messages: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<ChatMessageDto>>> GetSessionMessagesPaginatedAsync(
            Guid sessionId, Guid userId, int pageNumber, int pageSize)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<PaginatedResult<ChatMessageDto>>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<PaginatedResult<ChatMessageDto>>.FailureResult("You do not have permission to view this session");

                var messages = await _chatRepository.GetSessionMessagesPaginatedAsync(sessionId, pageNumber, pageSize);
                var totalCount = await _chatRepository.GetSessionMessagesCountAsync(sessionId);

                var dtos = messages.Select(MapToMessageDto).ToList();

                var result = new PaginatedResult<ChatMessageDto>
                {
                    Items = dtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ServiceResult<PaginatedResult<ChatMessageDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<ChatMessageDto>>.FailureResult($"Failed to get messages: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return ServiceResult<bool>.FailureResult("Chat session not found");

                if (session.UserId != userId)
                    return ServiceResult<bool>.FailureResult("You do not have permission to update this session");

                var success = await _chatRepository.MarkMessagesAsReadAsync(sessionId, DateTime.UtcNow);
                return ServiceResult<bool>.SuccessResult(success);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to mark messages as read: {ex.Message}");
            }
        }

        // Feedback
        public async Task<ServiceResult<ChatFeedbackDto>> AddFeedbackAsync(Guid userId, ChatFeedbackDto dto)
        {
            try
            {
                var message = await _chatRepository.GetMessageByIdAsync(dto.MessageId);
                if (message == null)
                    return ServiceResult<ChatFeedbackDto>.FailureResult("Message not found");

                var session = await _chatRepository.GetSessionByIdAsync(message.SessionId);
                if (session == null || session.UserId != userId)
                    return ServiceResult<ChatFeedbackDto>.FailureResult("You do not have permission to provide feedback for this message");

                var feedback = new ChatFeedback
                {
                    MessageId = dto.MessageId,
                    UserId = userId,
                    Rating = dto.Rating,
                    Comment = dto.Comment
                };

                var created = await _chatRepository.AddFeedbackAsync(feedback);

                var resultDto = new ChatFeedbackDto
                {
                    MessageId = created.MessageId,
                    Rating = created.Rating,
                    Comment = created.Comment
                };

                return ServiceResult<ChatFeedbackDto>.SuccessResult(resultDto, "Feedback submitted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatFeedbackDto>.FailureResult($"Failed to submit feedback: {ex.Message}");
            }
        }

        // AI Integration
        public async Task<ServiceResult<string>> GenerateBotResponseAsync(string userMessage, Guid? sessionId = null)
        {
            try
            {
                var response = await GenerateBotResponseInternal(userMessage, sessionId);
                return ServiceResult<string>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Failed to generate response: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> SummarizeSessionAsync(Guid sessionId)
        {
            try
            {
                var messages = await _chatRepository.GetSessionMessagesAsync(sessionId, 100);
                if (!messages.Any())
                    return ServiceResult<string>.FailureResult("No messages to summarize");

                var conversation = string.Join("\n", messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => $"{m.Sender}: {m.Message}"));

                var prompt = $"Summarize the following real estate conversation:\n\n{conversation}\n\nSummary:";
                var summary = await CallGeminiApiAsync(prompt);

                // Update session summary
                var session = await _chatRepository.GetSessionByIdAsync(sessionId);
                if (session != null)
                {
                    session.Summary = summary.Length > 500 ? summary[..500] : summary;
                    await _chatRepository.UpdateSessionAsync(session);
                }

                return ServiceResult<string>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Failed to summarize session: {ex.Message}");
            }
        }

        // Statistics
        public async Task<ServiceResult<ChatStatisticsDto>> GetChatStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var stats = await _chatRepository.GetChatStatisticsAsync(fromDate, toDate);
                var dailyActivity = await _chatRepository.GetDailyActivityAsync(30);
                var popularTopics = await _chatRepository.GetPopularTopicsAsync(10);

                var dto = new ChatStatisticsDto
                {
                    TotalSessions = stats.TotalSessions,
                    ActiveSessions = stats.ActiveSessions,
                    ArchivedSessions = stats.ArchivedSessions,
                    TotalMessages = stats.TotalMessages,
                    UserMessages = stats.UserMessages,
                    BotMessages = stats.BotMessages,
                    AverageSessionDuration = stats.AverageSessionDuration,
                    AverageMessagesPerSession = stats.AverageMessagesPerSession,
                    PopularTopics = popularTopics,
                    DailyActivity = dailyActivity.Select(d => new DailyChatActivityDto
                    {
                        Date = d.Date,
                        SessionCount = d.SessionCount,
                        MessageCount = d.MessageCount
                    }).ToList()
                };

                return ServiceResult<ChatStatisticsDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatStatisticsDto>.FailureResult($"Failed to get chat statistics: {ex.Message}");
            }
        }

        // Private helper methods
        private async Task<string> GenerateBotResponseInternal(string userMessage, Guid? sessionId = null, string? contextData = null)
        {
            var systemPrompt = "Bạn là một trợ lý ảo chuyên nghiệp tư vấn về bất động sản cho RealEstateListingPlatform. " +
                              "Hãy trả lời ngắn gọn, lịch sự bằng tiếng Việt. " +
                              "Nếu người dùng hỏi về các bất động sản cụ thể, hãy đề xuất các tin đăng phù hợp từ hệ thống.";

            var context = string.IsNullOrEmpty(contextData) ? "" : $"Context: {contextData}\n\n";
            var prompt = $"{systemPrompt}\n\n{context}Người dùng hỏi: {userMessage}";

            return await CallGeminiApiAsync(prompt);
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 1,
                        topP = 1,
                        maxOutputTokens = 2048
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentNode) &&
                        contentNode.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
                    }
                }

                return "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.";
            }
            catch
            {
                return "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.";
            }
        }

        private async Task UpdateSessionContextAsync(Guid sessionId, string userMessage, string botResponse)
        {
            // Simple context tracking - extract property-related keywords
            var session = await _chatRepository.GetSessionByIdAsync(sessionId);
            if (session == null) return;

            var context = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(session.ContextData))
            {
                try
                {
                    context = JsonSerializer.Deserialize<Dictionary<string, object>>(session.ContextData) ?? new();
                }
                catch { }
            }

            // Track mentioned property types
            var propertyTypes = new[] { "apartment", "house", "villa", "land", "office", "room" };
            foreach (var type in propertyTypes)
            {
                if (userMessage.Contains(type, StringComparison.OrdinalIgnoreCase))
                {
                    context["last_property_type"] = type;
                }
            }

            // Track mentioned locations
            if (userMessage.Contains("district", StringComparison.OrdinalIgnoreCase) ||
                userMessage.Contains("quận", StringComparison.OrdinalIgnoreCase))
            {
                // Simple location extraction - could be improved with NLP
                context["location_mentioned"] = true;
            }

            session.ContextData = JsonSerializer.Serialize(context);
            await _chatRepository.UpdateSessionAsync(session);
        }

        private async Task NotifyNewMessageAsync(Guid sessionId, ChatMessage message)
        {
            try
            {
                await _dashboardHub.Clients.Group($"chat-{sessionId}")
                    .ReceiveDashboardUpdate("NewChatMessage", new
                    {
                        sessionId,
                        messageId = message.Id,
                        sender = message.Sender,
                        message = message.Message,
                        sentAt = message.SentAt
                    });
            }
            catch
            {
                // SignalR failure shouldn't break message sending
            }
        }

        private async Task<ChatSessionDto> MapToDto(ChatSession session)
        {
            var user = await _userRepository.GetUserById(session.UserId);

            return new ChatSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                UserName = user?.DisplayName ?? "Unknown",
                Title = session.Title,
                Summary = session.Summary,
                StartedAt = session.StartedAt,
                LastMessageAt = session.LastMessageAt,
                MessageCount = session.MessageCount,
                Status = session.Status,
                ContextData = session.ContextData,
                RecentMessages = session.Messages?
                    .OrderByDescending(m => m.SentAt)
                    .Take(5)
                    .Select(MapToMessageDto)
                    .ToList() ?? new()
            };
        }

        private ChatMessageDto MapToMessageDto(ChatMessage message)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                SessionId = message.SessionId,
                Sender = message.Sender,
                Message = message.Message,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                MessageType = message.MessageType,
                Metadata = message.Metadata
            };
        }
    }
}