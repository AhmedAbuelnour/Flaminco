namespace Flaminco.ImmutableStates.Entity
{
    /// <summary>
    /// Represents an immutable state entity with an ID, code, group, descriptions, and metadata.
    /// </summary>
    public record class ImmutableState
    {
        /// <summary>
        /// Gets the ID of the immutable state entity.
        /// </summary>
        public int Id { get; init; }
        /// <summary>
        /// Gets the code of the immutable state entity.
        /// </summary>
        public int Code { get; init; }
        /// <summary>
        /// Gets the descriptions of the immutable state entity in different languages.
        /// </summary>
        public Dictionary<string, string> Descriptions { get; init; } = default!;
        /// <summary>
        /// Gets the group of the immutable state entity.
        /// </summary>
        public string? Group { get; init; }
        /// <summary>
        /// Gets the metadata associated with the immutable state entity.
        /// </summary>
        public string? Metadata { get; init; }
    }
}
