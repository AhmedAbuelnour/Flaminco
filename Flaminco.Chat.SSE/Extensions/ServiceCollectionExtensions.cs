using LowCodeHub.Chat.SSE.Contracts;
using LowCodeHub.Chat.SSE.Options;
using LowCodeHub.Chat.SSE.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LowCodeHub.Chat.SSE.Extensions;

/// <summary>
/// Extension methods for registering LowCodeHub.Chat.SSE services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis Streams + SSE chat services for plug-and-play use.
    /// </summary>
    public static IServiceCollection AddChatSse(
        this IServiceCollection services,
        Action<RedisChatSseOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<RedisChatSseService>();
        services.TryAddSingleton<IChatMessagePublisher>(sp => sp.GetRequiredService<RedisChatSseService>());
        services.TryAddSingleton<IChatSseStreamService>(sp => sp.GetRequiredService<RedisChatSseService>());

        return services;
    }

    /// <summary>
    /// Registers Redis Streams + SSE chat services and enforces caller-provided
    /// history-reader and heartbeat-factory implementations for a message type.
    /// </summary>
    public static IServiceCollection AddChatSse<TMessage, THistoryReader, THeartbeatFactory>(
        this IServiceCollection services,
        Action<RedisChatSseOptions> configure)
        where TMessage : class, IChatStreamMessage
        where THistoryReader : class, IChatHistoryReader<TMessage>
        where THeartbeatFactory : class, IChatHeartbeatFactory<TMessage>
    {
        services.AddChatSse(configure);
        services.TryAddSingleton<IChatHistoryReader<TMessage>, THistoryReader>();
        services.TryAddSingleton<IChatHeartbeatFactory<TMessage>, THeartbeatFactory>();
        return services;
    }
}

