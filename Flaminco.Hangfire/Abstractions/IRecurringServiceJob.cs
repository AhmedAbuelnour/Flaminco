namespace Flaminco.Hangfire.Abstractions;

public interface IRecurringServiceJob : IServiceJob
{
    string Key { get; init; }
    string RecurringCron { get; init; }
}