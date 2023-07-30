using System;

namespace Flaminco.Hangfire.Abstractions;

public interface IScheduleServiceJob : IServiceJob
{
    TimeSpan Offset { get; init; }
}
