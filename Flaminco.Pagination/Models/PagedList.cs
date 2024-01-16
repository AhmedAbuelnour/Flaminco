namespace Flaminco.Pagination.Models
{
    public class PagedList<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int? PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage => PageSize.HasValue && Page * PageSize < TotalCount;
        public bool HasPreviousPage => Page > 1;
    }
}
