using Flaminco.QueryableExtensions.Models;
using Microsoft.EntityFrameworkCore;

namespace Flaminco.QueryableExtensions.Extensions;

public static class PagedListExtensions
{
    public static async Task<PagedList<TItem>> ToPagedListAsync<TItem>(this IQueryable<TItem> query,
                                                                       int page,
                                                                       int pageSize,
                                                                       CancellationToken cancellationToken = default)
    {
        return new PagedList<TItem>
        {
            Page = page,
            PageSize = pageSize,
            Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
            TotalCount = await query.CountAsync(cancellationToken)
        };
    }

    public static async Task<AttachedPagedList<TItem, TAttachment>> ToAttachedPagedListAsync<TItem, TAttachment>(this IQueryable<TItem> query,
                                                                                                                 int page,
                                                                                                                 int pageSize,
                                                                                                                 TAttachment attachment,
                                                                                                                 CancellationToken cancellationToken = default)
    {
        return new AttachedPagedList<TItem, TAttachment>
        {
            Page = page,
            PageSize = pageSize,
            Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
            Attachment = attachment,
            TotalCount = await query.CountAsync(cancellationToken)
        };
    }
}