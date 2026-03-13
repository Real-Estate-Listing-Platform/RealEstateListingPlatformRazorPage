using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementation
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ChatSessionRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        // Session operations
        public async Task<ChatSession> CreateSessionAsync(ChatSession session)
        {
            session.Id = Guid.NewGuid();
            session.StartedAt = DateTime.UtcNow;
            session.LastMessageAt = null;
            session.MessageCount = 0;

            await _context.Set<ChatSession>().AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatSession?> GetSessionByIdAsync(Guid id)
        {
            return await _context.Set<ChatSession>()
                .Include(s => s.User)
                .Include(s => s.Messages.OrderByDescending(m => m.SentAt).Take(5))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(Guid userId, bool includeArchived = false)
        {
            var query = _context.Set<ChatSession>()
                .Include(s => s.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(s => s.UserId == userId);

            if (!includeArchived)
            {
                query = query.Where(s => s.Status == "Active");
            }

            return await query
                .OrderByDescending(s => s.LastMessageAt ?? s.StartedAt)
                .ToListAsync();
        }

        public async Task<List<ChatSession>> GetUserSessionsPaginatedAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? status = null)
        {
            var query = _context.Set<ChatSession>()
                .Include(s => s.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(s => s.UserId == userId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(searchTerm) ||
                    (s.Summary != null && s.Summary.ToLower().Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(s => s.LastMessageAt ?? s.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserSessionsCountAsync(Guid userId, string? searchTerm = null, string? status = null)
        {
            var query = _context.Set<ChatSession>()
                .Where(s => s.UserId == userId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(searchTerm) ||
                    (s.Summary != null && s.Summary.ToLower().Contains(searchTerm)));
            }

            return await query.CountAsync();
        }

        public async Task<ChatSession> UpdateSessionAsync(ChatSession session)
        {
            _context.Set<ChatSession>().Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<bool> DeleteSessionAsync(Guid id)
        {
            var session = await _context.Set<ChatSession>()
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return false;

            _context.Set<ChatMessage>().RemoveRange(session.Messages);
            _context.Set<ChatSession>().Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveSessionAsync(Guid id)
        {
            var session = await _context.Set<ChatSession>().FindAsync(id);
            if (session == null)
                return false;

            session.Status = "Archived";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreSessionAsync(Guid id)
        {
            var session = await _context.Set<ChatSession>().FindAsync(id);
            if (session == null)
                return false;

            session.Status = "Active";
            await _context.SaveChangesAsync();
            return true;
        }

        // Message operations
        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;

            await _context.Set<ChatMessage>().AddAsync(message);

            // Update session
            var session = await _context.Set<ChatSession>().FindAsync(message.SessionId);
            if (session != null)
            {
                session.LastMessageAt = message.SentAt;
                session.MessageCount++;
            }

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId, int limit = 50)
        {
            return await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetSessionMessagesPaginatedAsync(Guid sessionId, int pageNumber, int pageSize)
        {
            return await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSessionMessagesCountAsync(Guid sessionId)
        {
            return await _context.Set<ChatMessage>()
                .CountAsync(m => m.SessionId == sessionId);
        }

        public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, DateTime upTo)
        {
            var messages = await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId && m.SentAt <= upTo && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(Guid id)
        {
            return await _context.Set<ChatMessage>()
                .Include(m => m.Session)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Feedback
        public async Task<ChatFeedback> AddFeedbackAsync(ChatFeedback feedback)
        {
            feedback.Id = Guid.NewGuid();
            feedback.CreatedAt = DateTime.UtcNow;

            await _context.Set<ChatFeedback>().AddAsync(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<List<ChatFeedback>> GetMessageFeedbackAsync(Guid messageId)
        {
            return await _context.Set<ChatFeedback>()
                .Where(f => f.MessageId == messageId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForSessionAsync(Guid sessionId)
        {
            var query = from f in _context.Set<ChatFeedback>()
                        join m in _context.Set<ChatMessage>() on f.MessageId equals m.Id
                        where m.SessionId == sessionId
                        select f.Rating;

            if (!await query.AnyAsync())
                return 0;

            return await query.AverageAsync();
        }

        // Statistics
        public async Task<ChatStatistics> GetChatStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var start = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;

            var sessionsQuery = _context.Set<ChatSession>()
                .Where(s => s.StartedAt >= start && s.StartedAt <= end);

            var messagesQuery = _context.Set<ChatMessage>()
                .Where(m => m.SentAt >= start && m.SentAt <= end);

            var sessions = await sessionsQuery.ToListAsync();
            var messages = await messagesQuery.ToListAsync();

            var totalSessions = sessions.Count;
            var activeSessions = sessions.Count(s => s.Status == "Active");
            var archivedSessions = sessions.Count(s => s.Status == "Archived");
            var totalMessages = messages.Count;
            var userMessages = messages.Count(m => m.Sender == "User");
            var botMessages = messages.Count(m => m.Sender == "Bot");

            // Average session duration
            var sessionsWithDuration = sessions.Where(s => s.LastMessageAt.HasValue)
                .Select(s => (s.LastMessageAt.Value - s.StartedAt).TotalMinutes)
                .ToList();
            var avgDuration = sessionsWithDuration.Any() ? sessionsWithDuration.Average() : 0;

            // Average messages per session
            var avgMessages = sessions.Any() ? (double)totalMessages / sessions.Count : 0;

            return new ChatStatistics
            {
                TotalSessions = totalSessions,
                ActiveSessions = activeSessions,
                ArchivedSessions = archivedSessions,
                TotalMessages = totalMessages,
                UserMessages = userMessages,
                BotMessages = botMessages,
                AverageSessionDuration = avgDuration,
                AverageMessagesPerSession = avgMessages
            };
        }

        public async Task<List<DailyChatActivity>> GetDailyActivityAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var sessions = await _context.Set<ChatSession>()
                .Where(s => s.StartedAt >= startDate)
                .GroupBy(s => s.StartedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var messages = await _context.Set<ChatMessage>()
                .Where(m => m.SentAt >= startDate)
                .GroupBy(m => m.SentAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new List<DailyChatActivity>();
            var currentDate = startDate;

            while (currentDate <= DateTime.UtcNow.Date)
            {
                result.Add(new DailyChatActivity
                {
                    Date = currentDate,
                    SessionCount = sessions.FirstOrDefault(s => s.Date == currentDate)?.Count ?? 0,
                    MessageCount = messages.FirstOrDefault(m => m.Date == currentDate)?.Count ?? 0
                });
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetPopularTopicsAsync(int limit = 10)
        {
            // This is a simplified version - in production, you might want to use NLP or keyword extraction
            var messages = await _context.Set<ChatMessage>()
                .Where(m => m.Sender == "User" && m.MessageType == "Question")
                .OrderByDescending(m => m.SentAt)
                .Take(1000)
                .Select(m => m.Message)
                .ToListAsync();

            var topicKeywords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["apartment"] = 0,
                ["house"] = 0,
                ["villa"] = 0,
                ["land"] = 0,
                ["price"] = 0,
                ["rent"] = 0,
                ["buy"] = 0,
                ["location"] = 0,
                ["district"] = 0,
                ["city"] = 0,
                ["bedroom"] = 0,
                ["bathroom"] = 0,
                ["area"] = 0,
                ["size"] = 0,
                ["legal"] = 0,
                ["furniture"] = 0,
                ["direction"] = 0,
                ["payment"] = 0,
                ["loan"] = 0,
                ["mortgage"] = 0
            };

            foreach (var message in messages)
            {
                var lowerMessage = message.ToLower();
                foreach (var keyword in topicKeywords.Keys.ToList())
                {
                    if (lowerMessage.Contains(keyword))
                    {
                        topicKeywords[keyword]++;
                    }
                }
            }

            return topicKeywords
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(limit)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}