using Rpg.ModObjects.Time;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class SyncedModSet : ModSet
    {
        [JsonInclude] public string SyncedToId { get; init; }
        [JsonConstructor] protected SyncedModSet()
            : base()
        { }

        public SyncedModSet(string ownerId, string syncedToId, string name)
            : base(ownerId, name)
        {
            SyncedToId = syncedToId;
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            return GetSyncedToExpiry();
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            return GetSyncedToExpiry();
        }

        private LifecycleExpiry GetSyncedToExpiry()
        {
            var expiry = Graph.GetLifecycleObject(SyncedToId)?.Expiry ?? Expiry;
            return expiry;
        }
    }
}
