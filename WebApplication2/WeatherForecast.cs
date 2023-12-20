using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Cached.Abstractions;

namespace WebApplication2
{
    public class GetUsers : ICachedEndPointRequest
    {
        public string Key => "user";
        public TimeSpan? Expiration => null;

    }

    public class GetUsersHandler : IEndPointRequestHandler<GetUsers>
    {
        public async Task<IResult> Handle(GetUsers request, CancellationToken cancellationToken)
        {
            return Results.Ok("Ahmed");
        }
    }
}
