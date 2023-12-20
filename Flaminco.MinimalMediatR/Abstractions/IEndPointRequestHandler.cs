using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IEndPointRequestHandler<TEndPointRequest> : IRequestHandler<TEndPointRequest, IResult> where TEndPointRequest : IEndPointRequest
{
}

