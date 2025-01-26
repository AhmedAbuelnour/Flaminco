using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Extensions;
using Flaminco.StateMachine;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Validations
{
    public interface ITest
    {

    }

    public class Test : ITest
    {

    }

    public class Address
    {
        [MinLength(3, ErrorMessage = "something to show here ")]
        public string Name { get; set; }
        [Range(18, 55)]
        public int Age { get; set; }

    }


    public class AddAddressCommand : IEndPointRequest
    {
        [FromBody] public Address Address { get; set; }
    }

    public class AddAddressValidator : AbstractValidator<AddAddressCommand>
    {
        public AddAddressValidator()
        {
        }
    }


    public class MyCustomValidation : AbstractValidator<Address>
    {
        public MyCustomValidation()
        {
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.Age).GreaterThan(25);
        }
    }

    public class TestEvent : INotification
    {

    }

    public class TestEventHandler(ITest test) : ChannelConsumer<TestEvent>
    {
        public override async Task Consume(TestEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("TestEvent");

            await Task.Delay(5000);

            throw new Exception();

            Console.WriteLine("Done");
        }

        public override async Task Consume(TestEvent notification, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine("Consumed error");

            await Task.CompletedTask;
        }
    }


    public class AddAddressCommandHandler() : IEndPointRequestHandler<AddAddressCommand>
    {
        public async Task<IResult> Handle(AddAddressCommand request, CancellationToken cancellationToken)
        {
            // Should always start with Initial state and Initial value

            StateContext<string> context = new(new StateA(), "sttt");

            while (await context.ProcessStateMachineAsync(cancellationToken))
            {
                Console.WriteLine("Executing");
            }

            return Results.Ok();
        }
    }



    public class TestModule : IModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MediatePost<AddAddressCommand>("/person/add")
                .WithName("add-person");
        }
    }


}


