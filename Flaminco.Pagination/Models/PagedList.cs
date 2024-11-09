﻿namespace Flaminco.QueryableExtensions.Models;

public class PagedList<TItem>
{
    public required IEnumerable<TItem> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
