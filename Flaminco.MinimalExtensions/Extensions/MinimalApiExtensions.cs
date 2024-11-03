using LowCodeHub.MinimalExtensions.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace LowCodeHub.MinimalExtensions.Extensions;

public static class MinimalApiExtensions
{
    public static RouteHandlerBuilder WithDefaultQueryParameter(this RouteHandlerBuilder builder, string paramName,
        string defaultValue)
    {
        // This will modify the request pipeline for the specific route to include a default query value
        builder.Add(endpointBuilder =>
        {
            if (endpointBuilder.RequestDelegate is RequestDelegate originalDelegate)
                endpointBuilder.RequestDelegate = async context =>
                {
                    if (!context.Request.Query.ContainsKey(paramName))
                    {
                        // If the query parameter is not present, add the default value
                        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
                        {
                            { paramName, defaultValue }
                        });

                        // Combine the new and existing query parameters
                        context.Request.Query = new QueryCollection(context.Request.Query.Concat(queryCollection)
                            .ToDictionary(x => x.Key, x => x.Value));
                    }

                    // Call the original delegate
                    await originalDelegate(context);
                };
        });

        return builder;
    }

    public static RouteHandlerBuilder AddMaskFilter(this RouteHandlerBuilder builder)
    {
        // This will modify the request pipeline for the specific route to include a default query value
        builder.AddEndpointFilter<MaskFilter>();

        return builder;
    }
}