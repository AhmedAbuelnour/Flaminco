using Flaminco.MinimalEndpoints.Contexts;
using Microsoft.Extensions.Localization;

namespace Flaminco.MinimalEndpoints.Helpers;

/// <summary>
/// Helper class for resolving localized error messages.
/// Provides a unified approach for localization across ValidationFailure and EndpointResult.
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// Gets a localized message using the specified key and optional format arguments.
    /// </summary>
    /// <param name="key">The localization key (e.g., ErrorCode).</param>
    /// <param name="fallback">The fallback message if localization fails or key is not found.</param>
    /// <param name="args">Optional format arguments for string.Format().</param>
    /// <returns>The localized message, or the fallback if localization fails.</returns>
    public static string? Localize(string? key, string? fallback = null, params object[]? args)
    {
        if (string.IsNullOrEmpty(key) || LocalizationContext.Current is null)
            return fallback;

        try
        {
            LocalizedString localizedString;

            // If format arguments are provided, use the formatted indexer
            if (args is not null && args.Length > 0)
            {
                localizedString = LocalizationContext.Current[key, args];
            }
            else
            {
                localizedString = LocalizationContext.Current[key];
            }

            return localizedString.ResourceNotFound ? fallback : localizedString.Value;
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>
    /// Gets a localized message, returning the key as fallback if not found.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="args">Optional format arguments for string.Format().</param>
    /// <returns>The localized message, or the key if not found.</returns>
    public static string LocalizeOrKey(string key, params object[]? args)
    {
        return Localize(key, key, args) ?? key;
    }
}

