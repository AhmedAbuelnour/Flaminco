using MassTransit;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    /// Represents an abstract MassTransit consumer.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be consumed.</typeparam>
    public abstract class MessageConsumer<TMessage> : IConsumer<TMessage>
        where TMessage : class
    {
        /// <summary>
        /// Consumes the message of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <param name="context">The consume context.</param>
        /// <returns>A task that represents the asynchronous consume operation.</returns>
        public abstract Task Consume(ConsumeContext<TMessage> context);

        /// <summary>
        /// Handles errors that occur during message consumption.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">The consume context.</param>
        /// <returns>A task that represents the asynchronous error handling operation.</returns>
        public virtual Task Consume(Exception exception, ConsumeContext<TMessage> context) => Task.CompletedTask;

        async Task IConsumer<TMessage>.Consume(ConsumeContext<TMessage> context)
        {
            try
            {
                await Consume(context);
            }
            catch (Exception ex)
            {
                await Consume(ex, context);
                throw;
            }
        }
    }
}
