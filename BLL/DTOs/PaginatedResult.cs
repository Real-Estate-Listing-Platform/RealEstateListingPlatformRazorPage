using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public int StartIndex => (PageNumber - 1) * PageSize + 1;
        public int EndIndex => Math.Min(PageNumber * PageSize, TotalCount);
    }
}
