using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IEndpointHandler<TEndpointRequest> : IRequestHandler<TEndpointRequest, IResult> where TEndpointRequest : IEndpoint;

