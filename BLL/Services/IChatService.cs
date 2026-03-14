using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IChatService
    {
        // Session management
        Task<ServiceResult<ChatSessionDto>> CreateSessionAsync(Guid userId, CreateChatSessionDto dto);
        Task<ServiceResult<ChatSessionDto>> GetSessionAsync(Guid sessionId, Guid userId);
        Task<ServiceResult<List<ChatSessionSummaryDto>>> GetUserSessionsAsync(Guid userId, bool includeArchived = false);
        Task<ServiceResult<PaginatedResult<ChatSessionSummaryDto>>> GetUserSessionsPaginatedAsync(
            Guid userId, ChatHistoryFilterDto filter);
        Task<ServiceResult<bool>> ArchiveSessionAsync(Guid sessionId, Guid userId);
        Task<ServiceResult<bool>> RestoreSessionAsync(Guid sessionId, Guid userId);
        Task<ServiceResult<bool>> DeleteSessionAsync(Guid sessionId, Guid userId);

        // Message handling
        Task<ServiceResult<ChatMessageDto>> SendMessageAsync(Guid userId, SendMessageDto dto);
        Task<ServiceResult<List<ChatMessageDto>>> GetSessionMessagesAsync(Guid sessionId, Guid userId, int limit = 50);
        Task<ServiceResult<PaginatedResult<ChatMessageDto>>> GetSessionMessagesPaginatedAsync(
            Guid sessionId, Guid userId, int pageNumber, int pageSize);
        Task<ServiceResult<bool>> MarkMessagesAsReadAsync(Guid sessionId, Guid userId);

        // Feedback
        Task<ServiceResult<ChatFeedbackDto>> AddFeedbackAsync(Guid userId, ChatFeedbackDto dto);

        // AI Integration
        Task<ServiceResult<string>> GenerateBotResponseAsync(string userMessage, Guid? sessionId = null);
        Task<ServiceResult<string>> SummarizeSessionAsync(Guid sessionId);

        // Statistics (Admin only)
        Task<ServiceResult<ChatStatisticsDto>> GetChatStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}