using CacheManager.Core;
using Flaminco.AuditTrail.Core.Factory;
using Flaminco.AuditTrail.Core.Tracker;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Flaminco.AuditTrail.Memory.Tracker;

public class AuditTrailTracker<TIntermediate, TSource> : IAuditTrailTracker<TIntermediate, TSource> where TIntermediate : ITracker
{
    private readonly ICacheManager<AuditSnapshot> _cache;
    private readonly IAuditTrailMapperFactory _auditTrailMapperFactory;
    private readonly ILogger _logger;

    public AuditTrailTracker(ILogger<AuditTrailTracker<TIntermediate, TSource>> logger,
                            ICacheManager<AuditSnapshot> cache,
                            IAuditTrailMapperFactory auditTrailMapperFactory)
    {
        _cache = cache;
        _auditTrailMapperFactory = auditTrailMapperFactory;
        _logger = logger;
    }

    public async ValueTask<Guid> EnableTrackerAsync<TMapper>(TSource source, string primaryKeyValue, string userId)
    {
        TIntermediate intermediate = await MapperAsunc<TMapper>(source);

        string tableName = Validate(intermediate);

        _logger.LogInformation($"Generate a chase item with an audit Tracker with Id {intermediate.LiveTrackerId}, saved into audit trail storage");

        CacheItem<AuditSnapshot> snapshot = new CacheItem<AuditSnapshot>(intermediate.LiveTrackerId.ToString(), "Audit_Trail", new AuditSnapshot
        {
            PKValue = primaryKeyValue,
            OldValue = JsonSerializer.Serialize(intermediate),
            Id = intermediate.LiveTrackerId,
            TableName = tableName,
            UserId = userId
        }, ExpirationMode.Absolute, TimeSpan.FromHours(12));

        _cache.Add(snapshot);

        _logger.LogInformation($"Finish adding audit Tracker with Id {intermediate.LiveTrackerId} to audit trail storage");

        return intermediate.LiveTrackerId;
    }
    public async ValueTask<AuditSnapshot> GenerateSnapshotForAddAsync<TMapper>(TSource source, string primaryKeyValue, string userId)
    {
        TIntermediate intermediate = await MapperAsunc<TMapper>(source);

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
    public async ValueTask ValidateTrackerAsync<TMapper>(TSource source, ActionType actionType, Action<AuditSnapshot> onSnapshotGenerate)
    {
        TIntermediate intermediate = await MapperAsunc<TMapper>(source);

        Validate(intermediate);

        CacheItem<AuditSnapshot>? oldSnapshot = _cache.GetCacheItem(intermediate.LiveTrackerId.ToString(), "Audit_Trail");

        if (oldSnapshot == null || oldSnapshot.Value == null && oldSnapshot.IsExpired)
        {
            _logger.LogInformation($"Not found an audit trail entity by Live Tracker Id {intermediate.LiveTrackerId}");

            return;
        }

        string json = JsonSerializer.Serialize(intermediate);

        if (JToken.DeepEquals(json, oldSnapshot?.Value?.OldValue))
        {
            _logger.LogInformation($"No Modifications have been detected for tracker Id {intermediate.LiveTrackerId}");

            return;
        }

        _logger.LogInformation($"Modifications have been detected for tracker Id {intermediate.LiveTrackerId}");

        onSnapshotGenerate(new AuditSnapshot
        {
            Id = intermediate.LiveTrackerId,
            ActionType = actionType,
            OldValue = oldSnapshot?.Value?.OldValue,
            PKValue = oldSnapshot?.Value?.PKValue,
            TableName = oldSnapshot?.Value?.TableName,
            CreatedOn = DateTime.UtcNow,
            NewValue = oldSnapshot.Value.ActionType == ActionType.Delete ? string.Empty : json,
            Changes = oldSnapshot.Value.ActionType == ActionType.Update ? new JsonDiffPatch().Diff(json, oldSnapshot?.Value?.OldValue) : null,
            UserId = oldSnapshot?.Value?.UserId,
        });

        if (oldSnapshot.Value.ActionType != ActionType.Add)
        {
            _cache.Remove(intermediate.LiveTrackerId.ToString(), "Audit_Trail");
        }
    }
    private async ValueTask<TIntermediate> MapperAsunc<TMapper>(TSource source)
    {
        IAuditTrailMapper? mapper = _auditTrailMapperFactory.Create<TMapper>();

        if (mapper == null)
        {
            throw new NullReferenceException($"There is no mapper for type {typeof(TMapper)}");
        }

        return await mapper.Map<TSource, TIntermediate>(source);

    }
    private string Validate(TIntermediate intermediate)
    {
        if (intermediate.LiveTrackerId == Guid.Empty)
        {
            throw new ArgumentException("Live Tracker Id must have a value, it can't be empty");
        }

        _logger.LogInformation($"Add the audit Tracker with Id {intermediate.LiveTrackerId} to audit trail storage");


        if (typeof(TIntermediate).GetCustomAttributes(typeof(AuditAttribute.TableNameAttribute), true).FirstOrDefault() is not AuditAttribute.TableNameAttribute tableNameAttribute)
        {
            throw new Exception("You have not provided a tableName for the current aduit trail intermediate type");
        }

        return tableNameAttribute.Name;
    }
}