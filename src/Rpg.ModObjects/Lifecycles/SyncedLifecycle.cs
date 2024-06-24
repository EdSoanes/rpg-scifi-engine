using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public class SyncedLifecycle : BaseLifecycle
    {
        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var modSet = graph.Locate<ModSet>(mod?.OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.Expiry;
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(mod?.OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.Expiry;
                return Expiry;
            }

            return base.OnStartLifecycle(graph, currentTime, mod);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var modSet = graph.Locate<ModSet>(mod?.OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.Expiry;
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(mod?.OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.Expiry;
                return Expiry;
            }

            return base.OnUpdateLifecycle(graph, currentTime, mod);
        }
    }
}
