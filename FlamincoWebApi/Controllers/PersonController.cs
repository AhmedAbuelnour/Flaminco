using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.Pipeline.Abstractions;
using Flaminco.Pipeline.Attributes;
using Flaminco.Validation.Extensions;
using Flaminco.Validation.Models;
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

    public class GetPersonQueryValidation : IValidationHandler<GetPersonQuery>
    {
        public Result Handler(GetPersonQuery input)
        {
            if (!input.TryDataAnnotationValidate(out Error[] errors))
            {
                return Result.Failure(errors);
            }

            return Result.Success();
        }
    }

    public class Pipe
    {
        public int Value { get; set; }
    }

    [KeyedPipeline<Pipe>(Order = 1, KeyName = "TestGroup")]
    [KeyedPipeline<Pipe>(Order = 2, KeyName = "TestGroup2")]
    public class FirstPipeline : IPipelineHandler<Pipe>
    {
        public ValueTask Handler(Pipe source, CancellationToken cancellationToken = default)
        {
            source.Value = 1;
            return ValueTask.CompletedTask;
        }
    }


    [KeyedPipeline<Pipe>(Order = 2, KeyName = "TestGroup")]
    [KeyedPipeline<Pipe>(Order = 1, KeyName = "TestGroup2")]
    public class SecondPipeline : IPipelineHandler<Pipe>
    {
        public ValueTask Handler(Pipe source, CancellationToken cancellationToken = default)
        {
            source.Value = 2;
            return ValueTask.CompletedTask;
        }
    }

    [KeyedPipeline<Pipe>(Order = 3, KeyName = "TestGroup2")]
    public class TestPipeline : IPipelineHandler<Pipe>
    {
        public ValueTask Handler(Pipe source, CancellationToken cancellationToken = default)
        {
            source.Value = 3;
            return ValueTask.CompletedTask;
        }
    }



    public class GetPersonQueryHandler(IPipeline _pipeline) : IEndPointRequestHandler<GetPersonQuery>
    {
        public async Task<IResult> Handle(GetPersonQuery request, CancellationToken cancellationToken)
        {
            var pipe = new Pipe { Value = 0 };

            await _pipeline.ExecuteKeyedPipeline(pipe, "TestGroup2", cancellationToken);

            return Results.Ok(pipe);
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