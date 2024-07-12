using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    internal class StateStore : RpgBaseStore<string, State>
    {
        public StateStore(string entityId)
            : base(entityId) { }

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

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time)
        {
            base.OnStartLifecycle(graph, time);

            foreach (var state in Get())
            {
                var stateExpiry = state.Lifecycle.OnStartLifecycle(graph, time);
                if (stateExpiry == LifecycleExpiry.Active)
                {
                    var entity = graph.GetEntity<RpgEntity>(state.OwnerId)!;
                    var stateSets = entity.GetActiveConditionalStateInstances(state.Name);
                    if (!stateSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        entity.AddModSet(stateModSet);
                    }
                }
            }

            return LifecycleExpiry.Active;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint time)
        {
            base.OnUpdateLifecycle(graph, time);

            foreach (var state in Get())
            {
                var stateExpiry = state.Lifecycle.OnUpdateLifecycle(graph, time);
                var entity = graph.GetEntity<RpgEntity>(state.OwnerId)!;
                var stateSets = entity.GetActiveConditionalStateInstances(state.Name);

                if (stateExpiry == LifecycleExpiry.Active)
                {
                    if (!stateSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        entity.AddModSet(stateModSet);
                    }
                }
                else
                {
                    foreach (var modSet in stateSets)
                        graph.RemoveModSet(modSet.Id);
                }
            }

            return LifecycleExpiry.Active;
        }
    }
}
