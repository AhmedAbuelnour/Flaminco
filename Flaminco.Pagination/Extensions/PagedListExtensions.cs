using Flaminco.Pagination.Models;
using Microsoft.EntityFrameworkCore;

namespace Flaminco.Pagination.Extensions
{
    public static class PagedListExtensions
    {
        public static async Task<PagedList<TItem>> ToPagedList<TItem>(this IQueryable<TItem> query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return new PagedList<TItem>
            {
                Page = page,
                PageSize = pageSize,
                Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
                TotalCount = await query.CountAsync(cancellationToken),
            };
        }

        public static async Task<PagedList<TItem>> ToPagedList<TItem>(this IQueryable<TItem> query, CancellationToken cancellationToken = default)
        {
            return new PagedList<TItem>
            {
                Page = 1,
                Items = await query.ToListAsync(cancellationToken),
                TotalCount = await query.CountAsync(cancellationToken),
            };
        }
    }
}
