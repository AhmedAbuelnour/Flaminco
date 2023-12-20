using Flaminco.MinimalMediatR.Cached.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Flaminco.MinimalMediatR.Cached.Implementations
{
    public sealed class CachedEndPointPipelineBehavior<TRequest, TResponse>(IMemoryCache cacheService) : IPipelineBehavior<TRequest, TResponse> where TRequest : ICachedEndPointRequest
    {
        private readonly IMemoryCache _cacheService = cacheService;

        private readonly static TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return _cacheService.GetOrCreateAsync(request.Key, async entry =>
            {
                entry.SetAbsoluteExpiration(request.Expiration ?? DefaultExpiration);

                return await next();
            })!;
        }
    }

}
