namespace Flaminco.Identifier
{
    /// <summary>
    /// Represents a strongly typed identifier for an entity of type <typeparamref name="TEntity"/>.
    /// This struct provides type safety for entity identifiers by wrapping a <see cref="System.Guid"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type this identifier is for. Must be a non-nullable type.</typeparam>
    public record struct Identifier<TEntity> where TEntity : notnull
    {
        /// <summary>
        /// Gets the GUID value of the identifier.
        /// </summary>
        public Guid Value { get; private set; }

        /// <summary>
        /// Private constructor used to initialize the <see cref="Identifier{TEntity}"/> with a specified GUID value.
        /// </summary>
        /// <param name="value">The GUID value for the identifier.</param>
        private Identifier(Guid value) : this() => Value = value;

        /// <summary>
        /// Creates a new identifier with a new GUID value for the specified entity type.
        /// </summary>
        /// <returns>A new <see cref="Identifier{TEntity}"/> instance with a unique GUID.</returns>
        public static Identifier<TEntity> New() => new(Guid.NewGuid());

        /// <summary>
        /// Returns a string representation of the identifier, including the entity type name and the GUID value.
        /// </summary>
        /// <returns>A string that represents the current <see cref="Identifier{TEntity}"/>.</returns>
        public override readonly string ToString() => $"{typeof(TEntity).Name}: {Value}";

        /// <summary>
        /// Enables implicit conversion from an <see cref="Identifier{TEntity}"/> to a <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="id">The <see cref="Identifier{TEntity}"/> to convert.</param>
        public static implicit operator Guid(Identifier<TEntity> id) => id.Value;
    }
}
