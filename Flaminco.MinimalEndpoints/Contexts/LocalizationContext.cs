using Flaminco.MinimalEndpoints.Abstractions;

namespace Flaminco.MinimalEndpoints.Contexts
{
    /// <summary>
    /// Provides ambient context for localization using AsyncLocal to share IStringLocalizer across the async flow.
    /// </summary>
    public static class LocalizationContext
    {
        private static readonly AsyncLocal<IPropertyAwareTextLocator?> _propertyAware = new();

        /// <summary>
        /// Gets the current IPropertyAwareTextLocator from the async context.
        /// </summary>
        public static IPropertyAwareTextLocator? PropertyAware => _propertyAware.Value;

        /// <summary>
        /// Sets the IStringLocalizer for the current async context.
        /// </summary>
        /// <param name="localizer">The IStringLocalizer to set.</param>
        public static void SetCurrent(IPropertyAwareTextLocator? localizer)
        {
            _propertyAware.Value = localizer;
        }

        /// <summary>
        /// Clears the current IStringLocalizer from the async context.
        /// </summary>
        public static void Clear()
        {
            _propertyAware.Value = null;
        }
    }
}
