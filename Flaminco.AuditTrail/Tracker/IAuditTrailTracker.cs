namespace Flaminco.AuditTrail.Core.Tracker
{
    public interface IAuditTrailTracker<TIntermediate, TSource> where TIntermediate : ITracker
    {
        ValueTask<Guid> EnableTrackerAsync<TMapper>(TSource source, string primaryKeyValue, string userId);
        ValueTask<AuditSnapshot> GenerateSnapshotForAddAsync<TMapper>(TSource source, string primaryKeyValue, string userId);
        ValueTask ValidateTrackerAsync<TMapper>(TSource source, ActionType actionType, Action<AuditSnapshot> onSnapshotGenerate);
    }
}