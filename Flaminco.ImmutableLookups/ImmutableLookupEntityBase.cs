namespace Flaminco.ImmutableLookups
{
    /// <summary>
    /// Represents an immutable lookup entity with a unique identifier, code, and module.
    /// </summary>
    /// <typeparam name="TKey">The type of the unique identifier.</typeparam>
    /// <param name="Id">The unique identifier of the entity.</param>
    /// <param name="Code">The code associated with the entity.</param>
    /// <param name="Module">The module to which the entity belongs.</param>
    public abstract record ImmutableLookupEntityBase<TKey>(TKey Id, int Code, string Module) where TKey : IEquatable<TKey>;
}
