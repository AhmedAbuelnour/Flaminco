using Flaminco.RedisChannels.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Flaminco.RedisChannels.Implementations;

internal static class RedisChannelSerializer
{
    private sealed record RedisTypedEnvelope(string Type, string Payload);

    public static string Serialize<T>(T item, RedisStreamConfiguration config)
    {
        var itemType = item?.GetType() ?? typeof(T);

        if (!ShouldUseEnvelope(typeof(T), itemType, config))
            return JsonSerializer.Serialize(item);

        var payload = JsonSerializer.Serialize(item, itemType);
        var envelope = new RedisTypedEnvelope(
            itemType.AssemblyQualifiedName ?? itemType.FullName ?? itemType.Name,
            payload);

        return JsonSerializer.Serialize(envelope);
    }

    public static T? Deserialize<T>(string json, RedisStreamConfiguration config)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (NotSupportedException)
        {
            // Happens for interface/abstract targets, fallback to envelope-based polymorphic deserialization.
        }
        catch (JsonException)
        {
            // Might be envelope payload or malformed content; try envelope path next.
        }

        var envelope = JsonSerializer.Deserialize<RedisTypedEnvelope>(json);
        if (envelope is null || string.IsNullOrWhiteSpace(envelope.Type) || string.IsNullOrWhiteSpace(envelope.Payload))
            return default;

        var targetType = ResolveType(envelope.Type, config);
        if (targetType is null || !typeof(T).IsAssignableFrom(targetType))
            return default;

        var typedValue = JsonSerializer.Deserialize(envelope.Payload, targetType);
        return typedValue is T value ? value : default;
    }

    private static bool ShouldUseEnvelope(Type declaredType, Type runtimeType, RedisStreamConfiguration config)
    {
        if (!config.EnablePolymorphicSerialization)
            return false;

        return declaredType.IsInterface
               || declaredType.IsAbstract
               || declaredType != runtimeType;
    }

    private static Type? ResolveType(string typeName, RedisStreamConfiguration config)
    {
        var type = Type.GetType(typeName, throwOnError: false);
        if (type is not null)
            return type;

        foreach (Assembly assembly in config.KnownTypeAssemblies)
        {
            type = assembly.GetType(typeName, throwOnError: false);
            if (type is not null)
                return type;

            type = assembly.GetTypes().FirstOrDefault(t =>
                string.Equals(t.FullName, typeName, StringComparison.Ordinal)
                || string.Equals(t.Name, typeName, StringComparison.Ordinal));

            if (type is not null)
                return type;
        }

        return null;
    }
}
