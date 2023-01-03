using MediatR;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Contracts;

public interface IHttpRequest : IRequest<IResult>
{

}
