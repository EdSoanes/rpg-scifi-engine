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
                {
                    state.OnAdding(Graph);
                    Items.Add(state.Name, state);
                }
            }
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            base.OnStartLifecycle(graph, currentTime, mod);

            foreach (var state in Get())
            {
                var stateExpiry = state.Lifecycle.OnStartLifecycle(graph, currentTime);
                if (stateExpiry == LifecycleExpiry.Active)
                {
                    var entity = graph.GetEntity<RpgEntity>(state.OwnerId)!;
                    if (graph.GetModSet(entity, state.Name) == null)
                    {
                        var stateModSet = entity.CreateStateInstance(state.Name);
                        entity.AddModSet(stateModSet);
                    }
                }
            }

            return LifecycleExpiry.Active;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            base.OnUpdateLifecycle(graph, currentTime, mod);

            foreach (var state in Get())
            {
                var stateExpiry = state.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                var entity = graph.GetEntity<RpgEntity>(state.OwnerId)!;
                if (stateExpiry == LifecycleExpiry.Active)
                {
                    if (graph.GetModSet(entity, state.Name) == null)
                    {
                        var stateModSet = entity.CreateStateInstance(state.Name);
                        entity.AddModSet(stateModSet);
                    }
                }
                else
                {
                    var stateInstance = entity.GetStateInstance(state.Name);
                    if (stateInstance != null)
                        graph.RemoveModSet(stateInstance.Id);
                }
            }

            return LifecycleExpiry.Active;
        }
    }
}
