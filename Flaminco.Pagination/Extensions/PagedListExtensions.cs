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

    public static async Task<PagedList<TItem>> ToPagedListAsync<TItem>(this IQueryable<TItem> query, CancellationToken cancellationToken = default)
    {
        int count = await query.CountAsync(cancellationToken);

        return new PagedList<TItem>
        {
            Page = 1,
            PageSize = count,
            Items = await query.ToListAsync(cancellationToken),
            TotalCount = count
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

    public static async Task<AttachedPagedList<TItem, TAttachment>> ToAttachedPagedListAsync<TItem, TAttachment>(this IQueryable<TItem> query,
                                                                                                                 TAttachment attachment,
                                                                                                                 CancellationToken cancellationToken = default)
    {
        int count = await query.CountAsync(cancellationToken);

        return new AttachedPagedList<TItem, TAttachment>
        {
            Page = 1,
            PageSize = count,
            Items = await query.ToListAsync(cancellationToken),
            Attachment = attachment,
            TotalCount = count
        };
    }
}