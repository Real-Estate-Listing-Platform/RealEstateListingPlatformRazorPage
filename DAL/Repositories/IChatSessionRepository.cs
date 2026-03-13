using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IChatSessionRepository
    {
        // Session operations
        Task<ChatSession> CreateSessionAsync(ChatSession session);
        Task<ChatSession?> GetSessionByIdAsync(Guid id);
        Task<List<ChatSession>> GetUserSessionsAsync(Guid userId, bool includeArchived = false);
        Task<List<ChatSession>> GetUserSessionsPaginatedAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm = null, string? status = null);
        Task<int> GetUserSessionsCountAsync(Guid userId, string? searchTerm = null, string? status = null);
        Task<ChatSession> UpdateSessionAsync(ChatSession session);
        Task<bool> DeleteSessionAsync(Guid id);
        Task<bool> ArchiveSessionAsync(Guid id);
        Task<bool> RestoreSessionAsync(Guid id);

        // Message operations
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId, int limit = 50);
        Task<List<ChatMessage>> GetSessionMessagesPaginatedAsync(Guid sessionId, int pageNumber, int pageSize);
        Task<int> GetSessionMessagesCountAsync(Guid sessionId);
        Task<bool> MarkMessagesAsReadAsync(Guid sessionId, DateTime upTo);
        Task<ChatMessage?> GetMessageByIdAsync(Guid id);

        // Feedback
        Task<ChatFeedback> AddFeedbackAsync(ChatFeedback feedback);
        Task<List<ChatFeedback>> GetMessageFeedbackAsync(Guid messageId);
        Task<double> GetAverageRatingForSessionAsync(Guid sessionId);

        // Statistics
        Task<ChatStatistics> GetChatStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<DailyChatActivity>> GetDailyActivityAsync(int days);
        Task<Dictionary<string, int>> GetPopularTopicsAsync(int limit = 10);
    }

    public class ChatStatistics
    {
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int ArchivedSessions { get; set; }
        public int TotalMessages { get; set; }
        public int UserMessages { get; set; }
        public int BotMessages { get; set; }
        public double AverageSessionDuration { get; set; }
        public double AverageMessagesPerSession { get; set; }
    }

    public class DailyChatActivity
    {
        public DateTime Date { get; set; }
        public int SessionCount { get; set; }
        public int MessageCount { get; set; }
    }
}