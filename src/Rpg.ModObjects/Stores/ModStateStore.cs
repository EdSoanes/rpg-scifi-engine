using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Stores
{
    public class ModStateStore : ModBaseStore<string, ModState>
    {
        public string[] StateNames { get => Items.Values.Select(x => x.Name).ToArray(); }
        public string[] ActiveStateNames { get => StateNames.Where(IsActive).ToArray();  }

        public bool IsActive(string state)
            => Graph?.GetEntity(EntityId!)?.IsStateActive(state) ?? false;

        public bool SetActive(string state)
        {
            var modState = this[state];
            if (modState != null)
            {
                modState.SetActive();
                return true;
            }

            return false;
        }

        public bool SetInactive(string state)
        {
            var modState = Items[state];
            if (modState != null)
            {
                modState.SetInactive();
                return true;
            }

            return false;
        }

        public void Add(params ModState[] modStates)
        {
            foreach (var modState in modStates)
            {
                if (!Contains(modState))
                {
                    Items.Add(modState.Name, modState);
                    if (Graph != null)
                    {
                        var entity = Graph.GetEntity(EntityId!);
                        modState.OnGraphCreating(Graph!, entity!);
                    }
                }
            }
        }

        public override void OnGraphCreating(RpgGraph graph, RpgObject entity)
        {
            base.OnGraphCreating(graph, entity);
            foreach (var state in Get())
                state.OnGraphCreating(graph, entity);
        }

        public override void OnTurnChanged(int turn)
        {
            foreach (var modState in Get())
                modState.OnTurnChanged(turn);
        }

        public override void OnBeginEncounter()
        {
            foreach (var modState in Get())
                modState.OnBeginEncounter();
        }

        public override void OnEndEncounter()
        {
            foreach (var state in Get())
                state.OnEndEncounter();
        }
    }
}
