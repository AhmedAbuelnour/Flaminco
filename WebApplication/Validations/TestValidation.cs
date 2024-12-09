using Flaminco.MinimalMediatR.Abstractions;
using Flaminco.MinimalMediatR.Extensions;
using FluentValidation;
using MediatR;
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


    public class AddPersonCommand : IEndPointRequest
    {
        [FromBody] public Person Person { get; set; }
        [FromQuery] public string? Query { get; set; } = "default";
    }

    public class MyModelValidator : AbstractValidator<AddPersonCommand>
    {
        public MyModelValidator()
        {
            RuleFor(a => a.Person).DataAnnotations();
        }
    }


    public class MyCustomValidation : AbstractValidator<Person>
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

    public class TestEventHandler : INotificationHandler<TestEvent>
    {
        public async Task Handle(TestEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("TestEvent");

            await Task.Delay(5000);

            Console.WriteLine("Done");
        }
    }

    public class NotificationErrorHandler2(IChannelPublisher channelPublisher) : INotificationErrorHandler
    {
        public async ValueTask<bool> HandleAsync(INotification notification, Exception exception, CancellationToken cancellationToken)
        {
            await channelPublisher.PublishAsync(notification, cancellationToken);

            return true;
        }
    }

    public class AddPersonCommandHandler(IChannelPublisher publisher) : IEndPointRequestHandler<AddPersonCommand>
    {
        public async Task<IResult> Handle(AddPersonCommand request, CancellationToken cancellationToken)
        {
            await publisher.PublishAsync(new TestEvent(), cancellationToken);

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


