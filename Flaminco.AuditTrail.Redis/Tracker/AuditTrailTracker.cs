using Flaminco.AuditTrail.Core.Factory;
using Flaminco.AuditTrail.Core.Tracker;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Flaminco.AuditTrail.Redis.Tracker;

public class AuditTrailTracker<TIntermediate, TSource> : AuditTrackerBase<TIntermediate, TSource> where TIntermediate : ITracker
                                                                                                  where TSource : ITracker
{

    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;

    public AuditTrailTracker(ILogger<AuditTrailTracker<TIntermediate, TSource>> logger,
                            IDistributedCache cache,
                            IAuditTrailMapperFactory auditTrailMapperFactory)
                            : base(logger, auditTrailMapperFactory)
    {
        _cache = cache;
        _logger = logger;
    }

    public override async ValueTask<Guid> SetTrackerAsync<TMapper>(TSource source, string primaryKeyValue, string userId, CancellationToken cancellationToken = default)
    {
        TIntermediate intermediate = await MapperAsync<TMapper>(source);

        string tableName = Validate(intermediate);

        string? cahceResult = await _cache.GetStringAsync(intermediate.LiveTrackerId.ToString(), cancellationToken);

        if (string.IsNullOrEmpty(cahceResult))
        {
            _logger.LogInformation($"Generate a cache item with an audit Tracker with Id {intermediate.LiveTrackerId}, saved into audit trail storage");

            string cacheItemAsJson = JsonSerializer.Serialize(new AuditSnapshot
            {
                PKValue = primaryKeyValue,
                OldValue = JsonSerializer.Serialize(intermediate),
                Id = intermediate.LiveTrackerId,
                TableName = tableName,
                UserId = userId
            });

            await _cache.SetStringAsync(intermediate.LiveTrackerId.ToString(), cacheItemAsJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ExpireTimeSpan ?? TimeSpan.FromMinutes(30)
            }, cancellationToken);

            _logger.LogInformation($"Finish adding audit Tracker with Id {intermediate.LiveTrackerId} to audit trail storage");
        }

        return intermediate.LiveTrackerId;
    }
    public override async ValueTask GetTrackerResultAsync<TMapper>(TSource source, ActionType actionType, Action<AuditSnapshot> onSnapshotGenerate, CancellationToken cancellationToken = default)
    {

        if (actionType == ActionType.Add)
        {
            return;
        }

        TIntermediate intermediate = await MapperAsync<TMapper>(source);

        Validate(intermediate);

        string? cahceResult = await _cache.GetStringAsync(intermediate.LiveTrackerId.ToString(), cancellationToken);


        if (!string.IsNullOrEmpty(cahceResult))
        {
            AuditSnapshot? snapshot = JsonSerializer.Deserialize<AuditSnapshot>(cahceResult);

            string json = JsonSerializer.Serialize(intermediate);

            if (JToken.DeepEquals(json, snapshot?.OldValue))
            {
                _logger.LogInformation($"No Modifications have been detected for tracker Id {intermediate.LiveTrackerId}");

                return;
            }

            _logger.LogInformation($"Modifications have been detected for tracker Id {intermediate.LiveTrackerId}");

            onSnapshotGenerate(new AuditSnapshot
            {
                Id = intermediate.LiveTrackerId,
                ActionType = actionType,
                OldValue = snapshot?.OldValue,
                PKValue = snapshot?.PKValue,
                TableName = snapshot?.TableName,
                CreatedOn = DateTime.UtcNow,
                NewValue = actionType == ActionType.Delete ? string.Empty : json,
                Changes = actionType == ActionType.Update ? new JsonDiffPatch().Diff(json, snapshot?.OldValue) : null,
                UserId = snapshot?.UserId,
            });

            _cache.Remove(intermediate.LiveTrackerId.ToString());
        }
        else
        {
            _logger.LogInformation($"Not found an audit trail entity by Live Tracker Id {intermediate.LiveTrackerId}");

            return;

        }
    }
}