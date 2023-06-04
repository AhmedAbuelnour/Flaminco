namespace Flaminco.Hangfire.Abstractions;

public interface IRecurringServiceJob : IServiceJob {
    string Key { get; set; }
    string RecurringCron { get; set; }
}
