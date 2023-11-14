using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using FlamincoWebApi.Entities;
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

    public class GetPersonQueryHandler(UserDbContext _dbContext) : IEndPointRequestHandler<GetPersonQuery>
    {
        public async Task<IResult> Handle(GetPersonQuery request, CancellationToken cancellationToken)
        {
            var users = _dbContext.Set<User>().Where(a => a.Attributes.Length > 3).ToList();

            return Results.Ok(new Person
            {
                FirstName = "Ahmed",
                LastName = "Ramadan",
                Age = 16
            });
        }
    }


    public interface ClassFeature
    {
        static virtual void M() => Console.WriteLine("Default behavior");

        static abstract void Show();
    }
    public class Class1 : ClassFeature
    {

        public static void M() => Console.WriteLine("Default behavior 2");

        public static void Show()
        {
            throw new NotImplementedException();
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