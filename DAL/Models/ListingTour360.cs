using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models;

public partial class ListingTour360
{
    [Key]
    public Guid ListingId { get; set; }

    public string? Provider { get; set; }

    public string? EmbedUrl { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
