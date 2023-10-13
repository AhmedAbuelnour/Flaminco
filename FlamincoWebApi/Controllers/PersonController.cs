using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FlamincoWebApi.Controllers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EndPointAttribute : Attribute
    {
        public required string Template { get; set; }

        public required HttpVerb HttpVerb { get; set; }
    }

    public enum HttpVerb
    {
        Get,
        Post,
        Put,
        Delete,
        Patch
    }

    [EndPoint(Template = "/GetPerson", HttpVerb = HttpVerb.Get)]
    public class GetPersonQuery : IEndPointRequest
    {

    }

    public class GetPersonQueryHandler : IEndPointRequestHandler<GetPersonQuery>
    {
        public async Task<IResult> Handle(GetPersonQuery request, CancellationToken cancellationToken)
        {
            return Results.Ok(new Person
            {
                FirstName = "Ahmed",
                LastName = "Ramadan",
                Age = 16
            });
        }
    }


    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IManualMapper _mapper;
        public PersonController(IManualMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public PersonResponse Get()
        {
            return _mapper.Map<Person, PersonResponse>(new Person
            {
                Age = 1,
                FirstName = "Ahmed",
                LastName = "Abuelnour"
            });
        }
    }
}