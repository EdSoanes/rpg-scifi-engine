using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    internal class StateStore : RpgBaseStore<string, States.State>
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
            foreach (var state in states)
            {
                if (!Contains(state.Name))
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
                var oldExpiry = state.Expiry;
                var newExpiry = state.OnUpdateLifecycle(graph, currentTime);

                if (oldExpiry != LifecycleExpiry.Active && newExpiry == LifecycleExpiry.Active)
                    graph.AddMods(state.Mods.ToArray());

                else if (oldExpiry == LifecycleExpiry.Active && newExpiry != LifecycleExpiry.Active)
                    graph.RemoveMods(state.Mods.ToArray());
            }

            return expiry;
        }
    }
}
