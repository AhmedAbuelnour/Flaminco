namespace Flaminco.Pagination.Models;

public class PagedList<T>
{
    private int? pageSize;
    public required IEnumerable<T> Items { get; set; }
    public int Page { get; set; }

    public int PageSize
    {
        get => pageSize ?? TotalCount;
        set => pageSize = value;
    }

    public int TotalCount { get; set; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}