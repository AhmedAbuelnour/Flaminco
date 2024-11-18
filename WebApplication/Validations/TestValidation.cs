using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Validations
{
    public class Person
    {
        [MinLength(3)]
        public string Name { get; set; }
        [Range(18, 55)]
        public int Age { get; set; }

    }


    public class AddPersonCommand : IEndpoint
    {
        [FromBody] public Person Person { get; set; }
    }

    public class AddPersonCommandHandler : IEndpointHandler<AddPersonCommand>
    {
        public async Task<IResult> Handle(AddPersonCommand request, CancellationToken cancellationToken)
        {
            return Results.Ok();
        }
    }

    public class AddPersonCommandValidator : AbstractValidator<AddPersonCommand>
    {
        public AddPersonCommandValidator()
        {
            RuleFor(a => a.Person.Name).MinimumLength(2).WithErrorCode("less_than_2").WithMessage("Name length must be between 3 and 10");
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
