using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class ListingMedia
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    public string? MediaType { get; set; }

    public string? Url { get; set; }

    public int? SortOrder { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
