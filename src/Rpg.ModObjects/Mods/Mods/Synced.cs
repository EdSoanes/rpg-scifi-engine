using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Synced : Mod
    {
        [JsonConstructor] protected Synced()
            : base()
        { }

        public Synced(string ownerId)
            : base(nameof(Synced))
        {
            OwnerId = ownerId;
            Behavior = new Add();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            var oldExpiry = Expiry;
            Expiry = GetOwnerExpiry();
            if (oldExpiry != Expiry)
                Graph.OnPropUpdated(Target);
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var oldExpiry = Expiry;
            Expiry = GetOwnerExpiry();
            if (oldExpiry != Expiry)
                Graph.OnPropUpdated(Target);
            return Expiry;
        }

        private LifecycleExpiry GetOwnerExpiry()
        {
            var expiry = Graph.GetLifecycleObject(OwnerId)?.Expiry ?? LifecycleExpiry.Destroyed;
            return expiry;
        }
    }
}
