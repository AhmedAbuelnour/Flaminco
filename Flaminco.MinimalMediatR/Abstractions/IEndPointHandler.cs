using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IEndPointHandler<TEndPointRequest> : IRequestHandler<TEndPointRequest, IResult>
    where TEndPointRequest : IEndPoint
{
}