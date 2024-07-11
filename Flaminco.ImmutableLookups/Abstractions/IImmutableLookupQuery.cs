namespace Flaminco.ImmutableLookups.Abstractions
{
    /// <summary>
    /// Defines methods for querying immutable lookup entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the unique identifier.</typeparam>
    public interface IImmutableLookupQuery<TEntity, TKey> where TEntity : ImmutableLookupEntityBase<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets all entities grouped by module.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A dictionary of entities grouped by module.</returns>
        Task<Dictionary<string, List<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets entities by a specific module.
        /// </summary>
        /// <param name="module">The module to filter entities.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A list of entities belonging to the specified module.</returns>
        Task<List<TEntity>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets entities by an array of modules.
        /// </summary>
        /// <param name="modules">The array of modules to filter entities.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A list of entities belonging to the specified modules.</returns>
        Task<List<TEntity>> GetByModulesAsync(string[] modules, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an entity by its code and module.
        /// </summary>
        /// <param name="code">The code of the entity.</param>
        /// <param name="module">The module of the entity.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The entity with the specified code and module, or null if not found.</returns>
        Task<TEntity?> GetByCodeAsync(int code, string module, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The entity with the specified unique identifier, or null if not found.</returns>
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    }
}
