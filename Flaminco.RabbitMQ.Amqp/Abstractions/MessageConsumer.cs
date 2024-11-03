using MassTransit;

namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
///     Represents an abstract base class for consuming messages from a message queue.
///     Implements <see cref="IConsumer{TMessage}" /> and <see cref="IConsumer{Fault}" />.
/// </summary>
/// <typeparam name="TMessage">The type of the message to be consumed. Must implement <see cref="IMessage" />.</typeparam>
public abstract class MessageConsumer<TMessage> : IConsumer<TMessage>, IConsumer<Fault<TMessage>>
    where TMessage : class, IMessage
{
    /// <summary>
    ///     Consumes the faulted message of type <typeparamref name="TMessage" />.
    /// </summary>
    /// <param name="context">The context of the faulted message.</param>
    /// <returns>A completed task by default.</returns>
    public virtual Task Consume(ConsumeContext<Fault<TMessage>> context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Consumes the message of type <typeparamref name="TMessage" />.
    /// </summary>
    /// <param name="context">The context of the message being consumed.</param>
    /// <returns>A task that represents the asynchronous consume operation.</returns>
    public abstract Task Consume(ConsumeContext<TMessage> context);
}