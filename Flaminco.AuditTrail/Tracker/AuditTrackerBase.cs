using Flaminco.AuditTrail.Core.Factory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Flaminco.AuditTrail.Core.Tracker;

public abstract class AuditTrackerBase<TIntermediate, TSource> : IAuditTrailTracker<TIntermediate, TSource> where TIntermediate : ITracker
{
    private readonly IAuditTrailMapperFactory _auditTrailMapperFactory;
    private readonly ILogger _logger;

    public AuditTrackerBase(ILogger<AuditTrackerBase<TIntermediate, TSource>> logger,
                            IAuditTrailMapperFactory auditTrailMapperFactory)
    {
        _auditTrailMapperFactory = auditTrailMapperFactory;
        _logger = logger;
    }

    protected TimeSpan? ExpireTimeSpan { get; set; }
    public void SetExpireTime(TimeSpan timeSpan) => ExpireTimeSpan = timeSpan;
    public abstract ValueTask GetTrackerResultAsync<TMapper>(TSource source, ActionType actionType, Action<AuditSnapshot> onSnapshotGenerate, CancellationToken cancellationToken);
    public abstract ValueTask<Guid> SetTrackerAsync<TMapper>(TSource source, string primaryKeyValue, string userId, CancellationToken cancellationToken);
    public async ValueTask<AuditSnapshot> GenerateSnapshotForAddAsync<TMapper>(TSource source, string primaryKeyValue, string userId)
    {
        TIntermediate intermediate = await MapperAsync<TMapper>(source);

        string tableName = Validate(intermediate);

        return new AuditSnapshot
        {
            PKValue = primaryKeyValue,
            OldValue = default,
            NewValue = JsonSerializer.Serialize(intermediate),
            Id = intermediate.LiveTrackerId,
            TableName = tableName,
            ActionType = ActionType.Add,
            CreatedOn = DateTime.UtcNow,
            Changes = default,
            UserId = userId
        };
    }
    protected async ValueTask<TIntermediate> MapperAsync<TMapper>(TSource source)
    {
        IAuditTrailMapper? mapper = _auditTrailMapperFactory.Create<TMapper>();

        if (mapper == null)
        {
            throw new NullReferenceException($"There is no mapper for type {typeof(TMapper)}");
        }

        return await mapper.Map<TSource, TIntermediate>(source);

    }
    protected string Validate(TIntermediate intermediate)
    {
        if (intermediate.LiveTrackerId == Guid.Empty)
        {
            throw new ArgumentException("Live Tracker Id must have a value, it can't be empty");
        }

        _logger.LogInformation($"Add the audit Tracker with Id {intermediate.LiveTrackerId} to audit trail storage");


        if (typeof(TIntermediate).GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() is not TableNameAttribute tableNameAttribute)
        {
            throw new Exception("You have not provided a tableName for the current aduit trail intermediate type");
        }

        return tableNameAttribute.Name;
    }
}
