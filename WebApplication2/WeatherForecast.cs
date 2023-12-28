using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Cached.Abstractions;

namespace WebApplication2
{
    public class GetUsers : ICachedEndPointRequest
    {
        public HttpContext Context { get; set; }
        public string Key => $"user_{Context}";
        public TimeSpan? Expiration => TimeSpan.FromSeconds(5);

    }

    public class GetUsersHandler(IMapper _mapper) : IEndPointRequestHandler<GetUsers>
    {
        public async Task<IResult> Handle(GetUsers request, CancellationToken cancellationToken)
        {
            C b = await _mapper.MapAsync<A, C>(new A
            {
                Age = 1,
                Name = "ahmed"
            }, cancellationToken);


            return Results.Ok(b);
        }
    }




    public class AClassMapper : IMapAsyncHandler<A, B>
    {
        public Task<B> Handler(A source, CancellationToken cancellationToken)
        {
            return Task.FromResult<B>(null);
        }

    }



    public class A
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }


    public class B
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
    public class C
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
