using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Flaminco.MinimalEndpoints.Implementations;

/// <summary>
/// Factory for creating JsonStringLocalizer instances.
/// Implements the native IStringLocalizerFactory interface.
/// </summary>
internal sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;
    private readonly JsonStringLocalizer _localizer;

    public JsonStringLocalizerFactory(IOptions<LocalizationOptions> options)
    {
        _resourcesPath = options.Value.ResourcesPath ?? "Resources";
        _localizer = new JsonStringLocalizer(_resourcesPath);
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(Type resourceSource)
    {
        return _localizer;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(string baseName, string location)
    {
        return _localizer;
    }
}

