using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    public class StateStore : ModBaseStore<string, States.State>
    {
        public StateStore(string entityId)
            : base(entityId) { }

        public bool SetOn(string stateName)
        {
            var state = this[stateName];
            if (state != null)
            {
                state.On();
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

        public void Add(params State[] states)
        {
            if (Graph != null && Graph.Time.Current != TimePoints.BeginningOfTime)
                throw new InvalidOperationException("Cannot add states to an entity after time has begun");

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
            {
                var wasOn = state!.IsOn;
                state.OnStartLifecycle(graph, currentTime);

                if (wasOn && !state.IsOn)
                    graph.RemoveMods(state.Mods.ToArray());
                else if (!wasOn && state.IsOn)
                    graph.AddMods(state.Mods.ToArray());
            }

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime, mod);

            foreach (var state in Get())
            {
                var wasOn = state!.IsOn;
                state.OnUpdateLifecycle(graph, currentTime);

                if (wasOn && !state.IsOn)
                    graph.RemoveMods(state.Mods.ToArray());
                else if (!wasOn && state.IsOn)
                    graph.AddMods(state.Mods.ToArray());
            }

            return expiry;
        }
    }
}
