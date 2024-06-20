using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    public class StateStore<T> : ModBaseStore<string, StateModification<T>>
        where T : RpgObject
    {
        public StateStore(string entityId)
            : base(entityId) { }

        public bool SetOn(string state)
        {
            var modState = this[state];
            if (modState != null)
            {
                modState.On();
                return true;
            }

            return false;
        }

        public bool SetOff(string state)
        {
            var modState = Items[state];
            if (modState != null)
            {
                modState.Off();
                return true;
            }

            return false;
        }

        public void Add(params StateModification<T>[] states)
        {
            if (Graph.Time.TimeHasBegun)
                throw new InvalidOperationException("Cannot add states to an entity after TimeHasBegun");

            foreach (var state in states)
            {
                if (!Contains(state))
                    Items.Add(state.Name, state);
            }
        }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);
            foreach (var state in Get())
                state.OnBeginningOfTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnStartLifecycle(graph, currentTime, mod);

            foreach (var state in Get())
                state.OnStartLifecycle(graph, currentTime);

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime, mod);

            foreach (var state in Get())
                state.OnUpdateLifecycle(graph, currentTime);

            return expiry;
        }
    }
}
