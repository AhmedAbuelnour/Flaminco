using Flaminco.ImmutableLookups.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Flaminco.ImmutableLookups.Implementations
{
    /// <inheritdoc/>
    public sealed class ImmutableLookupQuery<TContext, TEntity, TKey>(TContext _context) : IImmutableLookupQuery<TEntity, TKey> where TContext : DbContext
                                                                                                                                where TEntity : ImmutableLookupEntityBase<TKey>
                                                                                                                                where TKey : IEquatable<TKey>
    {
        /// <inheritdoc/>
        public Task<Dictionary<string, List<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _context.Set<TEntity>().GroupBy(a => a.Module).ToDictionaryAsync(a => a.Key, a => a.ToList(), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TEntity?> GetByCodeAsync(int code, string module, CancellationToken cancellationToken = default)
        {
            return _context.Set<TEntity>().Where(a => a.Code == code && a.Module == module).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return _context.Set<TEntity>().Where(a => a.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<List<TEntity>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
        {
            return _context.Set<TEntity>().Where(a => a.Module == module).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<List<TEntity>> GetByModulesAsync(string[] modules, CancellationToken cancellationToken = default)
        {
            return _context.Set<TEntity>().Where(a => modules.Contains(a.Module)).ToListAsync(cancellationToken);
        }
    }
}
