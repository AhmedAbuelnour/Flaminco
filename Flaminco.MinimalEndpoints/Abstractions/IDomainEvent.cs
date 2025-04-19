namespace Flaminco.MinimalEndpoints.Abstractions
{
    public interface IDomainEvent;

    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        ValueTask Handle(TEvent domainEvent, CancellationToken cancellationToken);
    }

}
