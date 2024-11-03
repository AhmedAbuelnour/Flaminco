namespace Flaminco.RediPolly.Abstractions;

/// <summary>
///     This interface represents a message with a resilient unique identifier, known as the ResilientKey,
///     which is used to uniquely identify the message in a Redis store.The ResilientKey property is of type
///     Guid and ensures that each message can be reliably retrieved and managed within the store.
/// </summary>
public interface IResilientMessage
{
    /// <summary>
    ///     Resilient Key to Identify the message in the redis store.
    /// </summary>
    public Guid ResilientKey { get; set; }
}