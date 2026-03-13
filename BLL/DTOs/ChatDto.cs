using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public string Status { get; set; } = "Active";
        public string? ContextData { get; set; }
        public List<ChatMessageDto> RecentMessages { get; set; } = new();
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string? MessageType { get; set; }
        public string? Metadata { get; set; }
    }

    public class CreateChatSessionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? InitialMessage { get; set; }
    }

    public class SendMessageDto
    {
        public Guid SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MessageType { get; set; }
    }

    public class ChatFeedbackDto
    {
        public Guid MessageId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class ChatSessionSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public string Preview { get; set; } = string.Empty;
    }

    public class ChatHistoryFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ChatStatisticsDto
    {
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int ArchivedSessions { get; set; }
        public int TotalMessages { get; set; }
        public int UserMessages { get; set; }
        public int BotMessages { get; set; }
        public double AverageSessionDuration { get; set; } // in minutes
        public double AverageMessagesPerSession { get; set; }
        public Dictionary<string, int> PopularTopics { get; set; } = new();
        public List<DailyChatActivityDto> DailyActivity { get; set; } = new();
    }

    public class DailyChatActivityDto
    {
        public DateTime Date { get; set; }
        public int SessionCount { get; set; }
        public int MessageCount { get; set; }
    }
}