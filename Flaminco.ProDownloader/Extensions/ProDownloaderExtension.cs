using Flaminco.ProDownloader.HttpClients;
using Flaminco.ProDownloader.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ProDownloader.Extensions;

public static class ProDownloaderExtension
{
    public static IServiceCollection AddProDownloader(this IServiceCollection services)
    {
        services.AddHttpClient<ResumableClient>();
        services.AddHttpClient<FileProfileClient>();
        services.AddHttpClient<SegementProfileClient>();

        services.AddScoped<PipeController>();
        services.AddScoped<FileProfileCreator>();
        services.AddScoped<FileDownloader>();

        return services;
    }
}
