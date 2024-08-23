using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class SyncedModSet : ModSet
    {
        [JsonProperty] public string SyncedToId { get; init; }

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
            return GetOwnerExpiry();
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            return GetOwnerExpiry();
        }

        private LifecycleExpiry GetOwnerExpiry()
        {
            var expiry = Graph.GetLifecycleObject(OwnerId)?.Expiry ?? Expiry;
            return expiry;
        }
    }
}
