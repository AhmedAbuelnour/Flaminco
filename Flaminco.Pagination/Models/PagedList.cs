using System.Collections.Generic;

namespace Flaminco.Pagination.Models
{
    public class PagedList<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }
    }
}
