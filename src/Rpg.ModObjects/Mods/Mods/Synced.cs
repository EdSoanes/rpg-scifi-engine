using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Synced : Mod
    {
        [JsonConstructor] protected Synced()
            : base()
        { }

        public Synced(string ownerId)
            : base()
        {
            OwnerId = ownerId;
            Behavior = new Add();
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
