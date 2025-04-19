using Flaminco.ImmutableStates.Abstractions;
using Flaminco.ImmutableStates.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Flaminco.ImmutableStates.Implementations;

/// <summary>
///     Provides a default implementation for looking up immutable states using Entity Framework Core.
/// </summary>
/// <typeparam name="TContext">The type of the DbContext used for database operations.</typeparam>
public sealed class DefaultImmutableStateLookup<TContext>(TContext _dbContext) : IImmutableStateLookup where TContext : DbContext
{
    /// <inheritdoc />
    public Task<ImmutableState?> GetByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ImmutableState>()
                         .AsNoTracking()
                         .Where(a => a.Code == code)
                         .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<ImmutableState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ImmutableState>()
                         .AsNoTracking()
                         .Where(a => a.Id == id)
                         .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ImmutableState>> GetByGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ImmutableState>()
                         .AsNoTracking()
                         .Where(a => a.Group == group)
                         .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ImmutableState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ImmutableState>()
                         .AsNoTracking()
                         .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ImmutableState>> FindByPredicateAsync(Expression<Func<ImmutableState, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ImmutableState>()
                         .AsNoTracking()
                         .Where(expression)
                         .ToListAsync(cancellationToken);
    }
}