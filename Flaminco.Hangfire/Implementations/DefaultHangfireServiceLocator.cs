using System;
using System.Linq;
using System.Threading;
using Flaminco.Hangfire.Abstractions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Hangfire.Implementations;

public class DefaultHangfireServiceLocator : IHangfireServiceLocator
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceProvider _serviceProvider;

    public DefaultHangfireServiceLocator(IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient,
        IServiceProvider serviceProvider)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
        _serviceProvider = serviceProvider;
    }

    public string? Execute<TServiceJob>(string? value = default, string? parentId = default,
        CancellationToken cancellationToken = default) where TServiceJob : IServiceJob
    {
        var serviceJob = _serviceProvider.GetServices<IServiceJob>()
            ?.FirstOrDefault(a => a.GetType() == typeof(TServiceJob));

        if (serviceJob is IQueueServiceJob)
            return _backgroundJobClient.Enqueue<TServiceJob>(service => service.Execute(value, cancellationToken));

        if (serviceJob is IScheduleServiceJob scheduleJob)
            return _backgroundJobClient.Schedule<TServiceJob>(service => service.Execute(value, cancellationToken),
                scheduleJob.Offset);

        if (serviceJob is IContinueServiceJob)
        {
            if (string.IsNullOrEmpty(parentId))
                ArgumentException.ThrowIfNullOrEmpty(parentId);

            return _backgroundJobClient.ContinueJobWith<TServiceJob>(parentId,
                service => service.Execute(value, cancellationToken));
        }

        if (serviceJob is IRecurringServiceJob recurringJob)
        {
            _recurringJobManager.AddOrUpdate<TServiceJob>(recurringJob.Key,
                service => service.Execute(value, cancellationToken), recurringJob.RecurringCron);

            return recurringJob.Key;
        }

        throw new InvalidOperationException();
    }
}