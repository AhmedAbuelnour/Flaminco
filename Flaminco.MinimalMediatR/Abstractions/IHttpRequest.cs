using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Abstractions;

public interface IHttpRequest : IRequest<IResult>
{

}
