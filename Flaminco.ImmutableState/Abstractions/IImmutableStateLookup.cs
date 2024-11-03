using System.Linq.Expressions;
using Flaminco.ImmutableStates.Entity;

namespace Flaminco.ImmutableStates.Abstractions;

public interface IImmutableStateLookup
{
    /// <summary>
    ///     Retrieves an immutable state entity by its code asynchronously.
    /// </summary>
    /// <param name="code">The code of the immutable state entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the immutable state entity, if
    ///     found.
    /// </returns>
    Task<ImmutableState?> GetByCodeAsync(int code, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves an immutable state entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the immutable state entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the immutable state entity, if
    ///     found.
    /// </returns>
    Task<ImmutableState?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a list of immutable state entities by their group asynchronously.
    /// </summary>
    /// <param name="group">The group of the immutable state entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of immutable state
    ///     entities.
    /// </returns>
    Task<List<ImmutableState>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves all immutable state entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of all immutable state
    ///     entities.
    /// </returns>
    Task<List<ImmutableState>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a list of immutable state entities that satisfy the specified predicate asynchronously.
    /// </summary>
    /// <param name="expression">The predicate to filter the immutable state entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of immutable state entities
    ///     that match the predicate.
    /// </returns>
    Task<List<ImmutableState>> FindByPredicateAsync(Expression<Func<ImmutableState, bool>> expression,
        CancellationToken cancellationToken = default);
}