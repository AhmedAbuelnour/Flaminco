using Microsoft.Extensions.Localization;

namespace Flaminco.MinimalEndpoints.Contexts;

/// <summary>
/// Provides ambient context for localization using AsyncLocal to share IStringLocalizer across the async flow.
/// </summary>
public static class LocalizationContext
{
    private static readonly AsyncLocal<IStringLocalizer?> _current = new();

    /// <summary>
    /// Gets the current IStringLocalizer from the async context.
    /// </summary>
    public static IStringLocalizer? Current => _current.Value;

    /// <summary>
    /// Sets the IStringLocalizer for the current async context.
    /// </summary>
    /// <param name="localizer">The IStringLocalizer to set.</param>
    public static void SetCurrent(IStringLocalizer? localizer)
    {
        _current.Value = localizer;
    }

    /// <summary>
    /// Clears the current IStringLocalizer from the async context.
    /// </summary>
    public static void Clear()
    {
        _current.Value = null;
    }
}
