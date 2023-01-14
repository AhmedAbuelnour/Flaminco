using Flaminco.Hangfire.Abstractions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Flaminco.Hangfire.Implementations
{
    public class DefaultHangfireServiceLocator : IHangfireServiceLocator
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;
        public DefaultHangfireServiceLocator(IRecurringJobManager recurringJobManager, IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider)
        {
            _recurringJobManager = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
        }

        public string? Execute<TServiceJob>(string? parentId = default) where TServiceJob : IServiceJob
        {
            IServiceJob? serviceJob = _serviceProvider.GetServices<IServiceJob>()?.FirstOrDefault(a => a.GetType() == typeof(TServiceJob));

            if (serviceJob is IQueueServiceJob queueJob)
            {
                return _backgroundJobClient.Enqueue(() => queueJob.Execute());
            }
            else if (serviceJob is IScheduleServiceJob scheduleJob)
            {
                return _backgroundJobClient.Schedule(() => scheduleJob.Execute(), scheduleJob.Offset);
            }
            else if (serviceJob is IContinueServiceJob continueJob)
            {
                if (string.IsNullOrEmpty(parentId))
                    throw new ArgumentNullException(nameof(parentId));

                return _backgroundJobClient.ContinueJobWith(parentId, () => continueJob.Execute());
            }
            else if (serviceJob is IRecurringServiceJob recurringJob)
            {
                _recurringJobManager.AddOrUpdate(recurringJob.Key, () => recurringJob.Execute(), recurringJob.RecurringCron);

                return default;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

}
