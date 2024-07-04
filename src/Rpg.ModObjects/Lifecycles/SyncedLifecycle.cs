using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public class SyncedLifecycle : BaseLifecycle
    {
        [JsonProperty] public string OwnerId { get; private set; }

        [JsonConstructor] private SyncedLifecycle() { }

        public SyncedLifecycle(string ownerId)
        {
            OwnerId = ownerId;
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var state = graph.Locate<State>(OwnerId);
            if (state != null)
            {
                Expiry = state.Lifecycle.OnStartLifecycle(graph, currentTime);
                return Expiry;
            }

            var modSet = graph.Locate<ModSet>(OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.OnStartLifecycle(graph, currentTime);
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.OnStartLifecycle(graph, currentTime);
                return Expiry;
            }

            return base.OnStartLifecycle(graph, currentTime);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var state = graph.Locate<State>(OwnerId);
            if (state != null)
            {
                Expiry = state.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                return Expiry;
            }

            var modSet = graph.Locate<ModSet>(OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                return Expiry;
            }

            return base.OnUpdateLifecycle(graph, currentTime);
        }
    }
}
