using LowCodeHub.Chat.SSE.Options;

namespace LowCodeHub.Chat.SSE.Internal;

internal static class RedisStreamKeyBuilder
{
    public static string Build(RedisChatSseOptions options, string channel)
    {
        var sanitized = string.IsNullOrWhiteSpace(channel) ? "default" : channel.Trim().ToLowerInvariant();
        return $"{options.StreamKeyPrefix}{sanitized}";
    }
}

