using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public class RpgEntity : RpgObject
    {
        [JsonProperty] internal StateStore StateStore { get; private set; }
        [JsonProperty] internal ActionStore ActionStore { get; private set; }

        public RpgEntity()
            : base() 
        {
            StateStore = new StateStore(Id);
            ActionStore = new ActionStore(Id);
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        public bool IsStateOn(string state)
            => StateStore[state]?.IsOn ?? false;

        public bool SetStateOn(string state)
            => StateStore[state]?.On() ?? false;

        public bool SetStateOff(string state)
            => StateStore[state]?.Off() ?? false;

        public State[] GetStates()
            => StateStore.Get();

        public ModSet[] GetActiveConditionalStateSets(string stateName)
            => Graph!.GetModSets(this, (x) => x.Name == stateName && x.Lifecycle is SyncedLifecycle && x.Expiry == LifecycleExpiry.Active);

        public ModSet[] GetActiveManualStateSets(string stateName)
            => Graph!.GetModSets(this, (x) => x.Name == stateName && !(x.Lifecycle is SyncedLifecycle) && x.Expiry == LifecycleExpiry.Active);

        public Actions.Action? GetAction(string action)
            => ActionStore[action];

        public Actions.Action[] GetActions()
            => ActionStore.Get();

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);

            ActionStore.OnBeforeTime(graph, entity);
            StateStore.OnBeforeTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            StateStore.OnStartLifecycle(graph, currentTime);
            var expiry = base.OnStartLifecycle(graph, currentTime);
            ActionStore.OnStartLifecycle(graph, currentTime);

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            StateStore.OnUpdateLifecycle(graph, currentTime);
            var expiry = base.OnUpdateLifecycle(graph, currentTime);
            ActionStore.OnUpdateLifecycle(graph, currentTime);

            return expiry;
        }
    }
}
