using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Contracts;

public interface IHttpRequestHandler<THttpRequest> : IRequestHandler<THttpRequest, IResult> where THttpRequest : IHttpRequest
{
}
