using LowCodeHub.MinimalExtensions.DataManipulation;
using Microsoft.AspNetCore.Http;

namespace LowCodeHub.MinimalExtensions.Filters
{
    public class MaskFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            object? result = await next(context);

            if (result is IValueHttpResult { Value: not null } objectResult && PropertyMasker.HasMaskedProperties(objectResult.Value))
            {
                PropertyMasker.MaskProperties(objectResult.Value);
            }

            return result;
        }
    }

}
