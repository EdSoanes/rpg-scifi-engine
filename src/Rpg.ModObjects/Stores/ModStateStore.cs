using Newtonsoft.Json;

namespace Rpg.ModObjects.Stores
{
    public class ModStateStore : ModBaseStore<string, ModState>
    {
        public string[] AllStates { get => Items.Values.Select(x => x.Name).ToArray(); }
        public string[] ActiveStates { get => Items.Values.Where(x => x.IsApplied).Select(x => x.Name).ToArray(); }

        public bool ManuallyActivateState(string state)
        {
            var modState = Items[state];
            if (modState != null)
            {
                modState.SetActive();
                return true;
            }

            return false;
        }

        public void ManuallySetInactive()
        {
            foreach (var modState in Get())
                modState.SetInactive();
        }

        public bool ManuallyDeactivateState(string state)
        {
            var modState = Items[state];
            if (modState != null)
            {
                modState.SetInactive();
                return true;
            }

            return false;
        }

        public void Add<T>(ModState<T> modState)
            where T : ModObject
        {
            if (!Contains(modState))
            {
                Items.Add(modState.Name, modState);
                if (Graph != null)
                {
                    var entity = Graph.GetEntity(EntityId!);
                    modState.OnGraphCreating(Graph!, entity);
                }
            }
        }

        public override void OnGraphCreating(ModGraph graph, ModObject entity)
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
