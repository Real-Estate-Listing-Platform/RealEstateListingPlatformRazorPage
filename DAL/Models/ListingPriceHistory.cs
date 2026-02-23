using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public partial class ListingPriceHistory
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    [Precision(18, 2)]
    public decimal? OldPrice { get; set; }

    [Precision(18, 2)]
    public decimal? NewPrice { get; set; }

    public Guid? ChangedByUserId { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual User? ChangedByUser { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
