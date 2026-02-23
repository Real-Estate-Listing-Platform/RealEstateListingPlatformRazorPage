using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class ListingViewStats
    {
        public Guid ListingId { get; set; }
        public int TotalViews { get; set; }
        public int ViewsToday { get; set; }
        public int ViewsThisWeek { get; set; }
        public int ViewsThisMonth { get; set; }
        public List<DailyViewStat> DailyStats { get; set; } = new();
    }

    public class DailyViewStat
    {
        public string Date { get; set; } = string.Empty;
        public int Views { get; set; }
    }
}
