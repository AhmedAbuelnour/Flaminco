using Flaminco.Pagination.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flaminco.Pagination.Implementations
{
    public static class PagedListExtensions
    {
        public static async Task<PagedList<TItem>> CreateAsync<TItem>(this IQueryable<TItem> query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return new PagedList<TItem>
            {
                Page = page,
                Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
                TotalCount = await query.CountAsync(cancellationToken),
                PageSize = pageSize,
            };
        }
    }
}
