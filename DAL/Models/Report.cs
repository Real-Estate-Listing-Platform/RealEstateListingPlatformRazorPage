using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Report
{
    public Guid Id { get; set; }

    public Guid ReporterId { get; set; }

    public Guid ListingId { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public Guid? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User Reporter { get; set; } = null!;
}
