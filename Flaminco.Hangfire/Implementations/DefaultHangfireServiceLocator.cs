using Flaminco.Hangfire.Abstractions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;

namespace Flaminco.Hangfire.Implementations
{
    public class DefaultHangfireServiceLocator : IHangfireServiceLocator
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;
        public DefaultHangfireServiceLocator(IRecurringJobManager recurringJobManager,
                                             IBackgroundJobClient backgroundJobClient,
                                             IServiceProvider serviceProvider)
        {
            _recurringJobManager = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
        }

        public string? Execute<TServiceJob, TServiceValue>(TServiceValue? value = default, string? parentId = default, CancellationToken cancellationToken = default) where TServiceJob : IServiceJob
                                                                                                                                                                      where TServiceValue : IServiceValue
        {
            IServiceJob? serviceJob = _serviceProvider.GetServices<IServiceJob>()?.FirstOrDefault(a => a.GetType() == typeof(TServiceJob));

            switch (serviceJob)
            {
                case IQueueServiceJob queueJob:
                    {
                        return _backgroundJobClient.Enqueue<TServiceJob>(service => service.Execute(value, cancellationToken));
                    }

                case IScheduleServiceJob scheduleJob:
                    {
                        return _backgroundJobClient.Schedule<TServiceJob>(service => service.Execute(value, cancellationToken), scheduleJob.Offset);
                    }

                case IContinueServiceJob continueJob:
                    {
                        if (string.IsNullOrEmpty(parentId))
                            throw new ArgumentNullException(nameof(parentId));

                        return _backgroundJobClient.ContinueJobWith<TServiceJob>(parentId, service => service.Execute(value, cancellationToken));
                    }

                case IRecurringServiceJob recurringJob:
                    {
                        _recurringJobManager.AddOrUpdate<TServiceJob>(recurringJob.Key, service => service.Execute(value, cancellationToken), recurringJob.RecurringCron);

                        return default;
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        public string? Execute<TServiceJob>(string? parentId = null, CancellationToken cancellationToken = default) where TServiceJob : IServiceJob
        {
            IServiceJob? serviceJob = _serviceProvider.GetServices<IServiceJob>()?.FirstOrDefault(a => a.GetType() == typeof(TServiceJob));

            switch (serviceJob)
            {
                case IQueueServiceJob queueJob:
                    {
                        return _backgroundJobClient.Enqueue<TServiceJob>(service => service.Execute<IServiceValue>(default, cancellationToken));
                    }

                case IScheduleServiceJob scheduleJob:
                    {
                        return _backgroundJobClient.Schedule<TServiceJob>(service => service.Execute<IServiceValue>(default, cancellationToken), scheduleJob.Offset);
                    }

                case IContinueServiceJob continueJob:
                    {
                        if (string.IsNullOrEmpty(parentId))
                            throw new ArgumentNullException(nameof(parentId));

                        return _backgroundJobClient.ContinueJobWith<TServiceJob>(parentId, service => service.Execute<IServiceValue>(default, cancellationToken));
                    }

                case IRecurringServiceJob recurringJob:
                    {
                        _recurringJobManager.AddOrUpdate<TServiceJob>(recurringJob.Key, service => service.Execute<IServiceValue>(default, cancellationToken), recurringJob.RecurringCron);

                        return default;
                    }

                default:
                    throw new InvalidOperationException();
            }
        }
    }

}
