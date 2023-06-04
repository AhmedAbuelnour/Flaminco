using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IHttpRequestHandler<THttpRequest> : IRequestHandler<THttpRequest, IResult> where THttpRequest : IHttpRequest
{
}
