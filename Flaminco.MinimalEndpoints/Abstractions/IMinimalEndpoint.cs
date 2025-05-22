using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Flaminco.MinimalEndpoints.Abstractions
{

    /// <summary>
    /// Represents a minimal API endpoint that can be registered with the route builder.
    /// </summary>
    public interface IMinimalRouteEndpoint
    {
        /// <summary>
        /// Adds the endpoint route to the route builder.
        /// </summary>
        /// <param name="app">The endpoint route builder</param>
        void AddRoute(IEndpointRouteBuilder app);
    }

    /// <summary>
    /// Represents a minimal API endpoint with no request parameters.
    /// </summary>
    public interface IMinimalEndpoint : IMinimalRouteEndpoint
    {
        /// <summary>
        /// Handles the endpoint request with no input parameters.
        /// </summary>
        /// <param name="cancellationToken">A token to observe for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP result</returns>
        ValueTask<IResult> Handle(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a minimal API endpoint with a single request parameter.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request parameter</typeparam>
    public interface IMinimalEndpoint<TRequest> : IMinimalRouteEndpoint
    {
        /// <summary>
        /// Handles the endpoint request with the specified input parameter.
        /// </summary>
        /// <param name="request">The request object</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP result</returns>
        ValueTask<IResult> Handle(TRequest request, CancellationToken cancellationToken);


    }

    /// <summary>
    /// Represents a minimal API endpoint with a request parameter and a strongly-typed result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request parameter</typeparam>
    /// <typeparam name="TResult">The type of the result</typeparam>
    public interface IMinimalEndpoint<TRequest, TResult> : IMinimalRouteEndpoint
    {
        /// <summary>
        /// Handles the endpoint request with the specified input parameter.
        /// </summary>
        /// <param name="request">The request object</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation, containing the strongly-typed result</returns>
        ValueTask<TResult> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
