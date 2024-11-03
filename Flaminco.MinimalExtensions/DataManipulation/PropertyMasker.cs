using System.Collections.Concurrent;
using System.Reflection;
using LowCodeHub.MinimalExtensions.Attributes;

namespace LowCodeHub.MinimalExtensions.DataManipulation;

public class PropertyMasker
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _maskedPropertiesCache = new();

    public static void MaskProperties(object obj)
    {
        foreach (var property in GetMaskedProperties(obj.GetType()))
            if (property.GetCustomAttribute<MaskedAttribute>() is MaskedAttribute maskedAttr)
            {
                var originalValue = (string?)property.GetValue(obj);
                var maskedValue = MaskString(originalValue, maskedAttr.Start, maskedAttr.Length,
                    maskedAttr.MaskingChar);
                property.SetValue(obj, maskedValue);
            }
    }

    public static bool HasMaskedProperties(object obj)
    {
        return GetMaskedProperties(obj.GetType()).Any();
    }


    private static List<PropertyInfo> GetMaskedProperties(Type type)
    {
        if (!_maskedPropertiesCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(MaskedAttribute)) && p.PropertyType == typeof(string))
                .ToList();

            _maskedPropertiesCache[type] = properties;
        }

        return properties;
    }

    private static string? MaskString(string? input, int start, int length, char maskingChar)
    {
        if (input == null) return null;
        if (start < 0 || length < 0 || start + length > input.Length)
            return input; // Consider adding error logging or handling here

        return string.Concat(input.AsSpan(0, start), new string(maskingChar, length), input.AsSpan(start + length));
    }
}