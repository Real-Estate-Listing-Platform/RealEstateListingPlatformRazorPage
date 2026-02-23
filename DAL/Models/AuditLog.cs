using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public string? ActionType { get; set; }

    public string? TargetType { get; set; }

    public Guid? TargetId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
