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


    public class AddAddressCommandHandler(StateContext<StateObject> stateContext, StateContext<StateObject2> stateContext2) : IEndPointRequestHandler<AddAddressCommand>
    {
        public async Task<IResult> Handle(AddAddressCommand request, CancellationToken cancellationToken)
        {
            // Should always start with Initial state and Initial value

            stateContext.SetState(nameof(StateA), new StateObject
            {
                Name = "Test 1"
            });

            await stateContext.ProcessAsync(cancellationToken);


            stateContext2.SetState(nameof(StateA), new StateObject2
            {
                Name = "Test 2"
            });

            await stateContext2.ProcessAsync(cancellationToken);


            foreach (var item in stateContext.StateSnapshots)
            {
                Console.WriteLine(item.ToString());
            }

            return Results.Ok();
        }
    }

    public class StateObject
    {
        public string Name { get; set; }
    }

    public class StateObject2
    {
        public string Name { get; set; }
    }

    [StateKey(nameof(StateA))]
    public class StateA(ILogger<StateA> logger) : State<StateObject>(logger)
    {
        public override async ValueTask<bool> Handle(StateContext<StateObject> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("State A");

            context.Payload.Name = "State A";

            context.SetState(nameof(StateB));

            return true;
        }
        public override async ValueTask<bool> Handle(StateContext<StateObject> context, Exception exception, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("State A Error");

            return false;
        }
    }

    [StateKey(nameof(StateB))]
    public class StateB(ILogger<StateB> logger) : State<StateObject>(logger)
    {
        public override async ValueTask<bool> Handle(StateContext<StateObject> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("State B");

            context.Payload.Name = "State B";

            return false;
        }
        public override async ValueTask<bool> Handle(StateContext<StateObject> context, Exception exception, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("State B Error");
            return false;
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


