using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class ChatSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }

        public int MessageCount { get; set; } = 0;

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Archived, Deleted

        [StringLength(500)]
        public string? ContextData { get; set; } // JSON string for storing session context (e.g., last searched properties)

        public virtual User User { get; set; } = null!;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public string Sender { get; set; } = string.Empty; // "User" or "Bot"

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string? MessageType { get; set; } // "Text", "PropertySuggestion", "Question", etc.

        [StringLength(1000)]
        public string? Metadata { get; set; } // JSON for additional data (property IDs, links, etc.)

        public virtual ChatSession Session { get; set; } = null!;
    }

    public class ChatFeedback
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MessageId { get; set; }

        public Guid? UserId { get; set; }

        public int Rating { get; set; } // 1-5 stars

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ChatMessage Message { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}