﻿using System.Net.Mime;
using Flaminco.RabbitMQ.AMQP.Models;
using MassTransit;

namespace Flaminco.RabbitMQ.AMQP.Abstractions;

/// <summary>
///     Represents an abstract base class for publishing messages to a message queue.
/// </summary>
/// <param name="sendEndpointProvider">The endpoint provider used to send messages to a specific queue.</param>
public abstract class MessagePublisher(ISendEndpointProvider sendEndpointProvider)
{
    /// <summary>
    ///     Gets the name of the queue where the message will be published.
    /// </summary>
    protected abstract string Queue { get; }

    /// <summary>
    ///     Publishes a message to the specified queue using MassTransit.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being published. Must implement <see cref="IMessage" />.</typeparam>
    /// <param name="message">The message to be published.</param>
    /// <param name="options">Optional parameters to customize the message sending process.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public async Task PublishAsync<TMessage>(TMessage message, MessagePublishOptions? options = default,
        CancellationToken cancellationToken = default) where TMessage : class, IMessage
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{Queue}"));


        await endpoint.Send(message, context => AttachProperties(context, options), cancellationToken);
    }

    /// <summary>
    ///     Publishes a message to the specified queue using MassTransit.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being published. Must implement <see cref="IMessage" />.</typeparam>
    /// <param name="message">The message to be published.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{Queue}"));

        await endpoint.Send(message, cancellationToken);
    }

    private static void AttachProperties<TMessage>(SendContext<TMessage> context, MessagePublishOptions? options)
        where TMessage : class, IMessage
    {
        if (options == null) return;
        // Apply properties if they are passed

        foreach (var property in options.ApplicationProperties ?? []) context.Headers.Set(property.Key, property.Value);

        if (options.MessageId.HasValue) context.MessageId = options.MessageId;

        if (options.CorrelationId.HasValue) context.CorrelationId = options.CorrelationId;

        if (options.TimeToLive.HasValue) context.TimeToLive = options.TimeToLive.Value;

        if (!string.IsNullOrWhiteSpace(options.PartitionKey)) context.SetPartitionKey(options.PartitionKey);

        if (!string.IsNullOrWhiteSpace(options.ContentType)) context.ContentType = new ContentType(options.ContentType);
    }
}