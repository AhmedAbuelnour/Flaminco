using MassTransit;

namespace Flaminco.RabbitMQ.AMQP.Abstractions
{
    /// <summary>
    ///     Represents an abstract base class for publishing messages to a message queue with request-response capabilities.
    ///     Provides methods to send a message and wait for a response, supporting different response types.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being published. Must implement <see cref="IMessage" />.</typeparam>
    /// <param name="requestClient">The request client used to send messages and receive responses from a specific queue.</param>
    public abstract class SyncMessagePublisher<TMessage>(IRequestClient<TMessage> requestClient)
        where TMessage : class, IMessage
    {
        /// <summary>
        ///     Sends a message to the queue and waits for a single response of type <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">An optional token to cancel the request operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the response of type <typeparamref name="TResponse"/>.</returns>
        public async Task<Response<TResponse>> GetResponseAsync<TResponse>(TMessage message, CancellationToken cancellationToken = default)
            where TResponse : class, IMessage
        {
            return await requestClient.GetResponse<TResponse>(message, cancellationToken);
        }

        /// <summary>
        ///     Sends a message to the queue and waits for a response with two possible types: <typeparamref name="TResponse1"/> or <typeparamref name="TResponse2"/>.
        /// </summary>
        /// <typeparam name="TResponse1">The first possible type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <typeparam name="TResponse2">The second possible type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">An optional token to cancel the request operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the response, which can be of type <typeparamref name="TResponse1"/> or <typeparamref name="TResponse2"/>.</returns>
        public async Task<Response<TResponse1, TResponse2>> GetResponseAsync<TResponse1, TResponse2>(TMessage message, CancellationToken cancellationToken = default)
            where TResponse1 : class, IMessage
            where TResponse2 : class, IMessage
        {
            return await requestClient.GetResponse<TResponse1, TResponse2>(message, cancellationToken);
        }

        /// <summary>
        ///     Sends a message to the queue and waits for a response with three possible types: <typeparamref name="TResponse1"/>, <typeparamref name="TResponse2"/>, or <typeparamref name="TResponse3"/>.
        /// </summary>
        /// <typeparam name="TResponse1">The first possible type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <typeparam name="TResponse2">The second possible type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <typeparam name="TResponse3">The third possible type of the expected response message. Must implement <see cref="IMessage" />.</typeparam>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">An optional token to cancel the request operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the response, which can be of type <typeparamref name="TResponse1"/>, <typeparamref name="TResponse2"/>, or <typeparamref name="TResponse3"/>.</returns>
        public async Task<Response<TResponse1, TResponse2, TResponse3>> GetResponseAsync<TResponse1, TResponse2, TResponse3>(TMessage message, CancellationToken cancellationToken = default)
            where TResponse1 : class, IMessage
            where TResponse2 : class, IMessage
            where TResponse3 : class, IMessage
        {
            return await requestClient.GetResponse<TResponse1, TResponse2, TResponse3>(message, cancellationToken);
        }
    }
}
