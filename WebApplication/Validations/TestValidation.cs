using Flaminco.ManualMapper.Abstractions;
using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Validations
{
    public class Person
    {
        [MinLength(3, ErrorMessage = "something to show here ")]
        public string Name { get; set; }
        [Range(18, 55)]
        public int Age { get; set; }

    }


    public class AddPersonCommand : IEndPoint
    {
        [FromBody] public Person Person { get; set; }
    }

    public class MyModelValidator : AbstractValidator<AddPersonCommand>
    {
        public MyModelValidator()
        {
            RuleFor(a => a.Person).DataAnnotations();
        }
    }

    public class AddPersonCommandHandler(IEntityDtoAdapter<Entity, Dto> _entityDtoAdapter) : IEndPointHandler<AddPersonCommand>
    {
        public async Task<IResult> Handle(AddPersonCommand request, CancellationToken cancellationToken)
        {
            Dto dto = new Dto
            {
                Id = 1,
                FullName = "Ahmed Abuelnour"
            };

            Entity entity = _entityDtoAdapter.ToEntity(dto);

            return Results.Ok();
        }
    }



    public class TestModule : IModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MediatePost<AddPersonCommand>("/person/add")
                .WithName("add-person");
        }
    }


}


