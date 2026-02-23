using System;

namespace BLL.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? ActorUserId { get; set; }
        public string ActorUserName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
