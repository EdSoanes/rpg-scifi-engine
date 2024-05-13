using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModStateStore : ITemporal
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public Guid? EntityId { get; set; }
        [JsonProperty] protected List<ModState> States { get; private set; } = new List<ModState>();

        public ModStateStore() { }

        public string[] AllStates { get => States.Select(x => x.Name).ToArray(); }
        public string[] ActiveStates { get => States.Where(x => x.IsApplied).Select(x => x.Name).ToArray(); }

        public bool SetActive(string state)
        {
            var modState = States.FirstOrDefault(x => x.Name == state);
            if (modState != null)
            {
                modState.SetActive();
                return true;
            }

            return false;
        }

        public bool SetInactive(string state)
        {
            var modState = States.FirstOrDefault(x => x.Name == state);
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
            if (!States.Contains(modState))
            {
                States.Add(modState);
                if (Graph != null)
                {
                    var entity = Graph.GetEntity(EntityId!);
                    modState.OnGraphCreating(Graph!, entity);
                }
            }
        }

        public ModState[] All()
            => States.ToArray();

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            EntityId = entity?.Id;

            foreach (var state in States)
                state.OnGraphCreating(graph, entity);
        }

        public void OnTurnChanged(int turn)
        {
            foreach (var modState in States)
                modState.OnTurnChanged(turn);
        }

        public void OnBeginEncounter()
        {
            foreach (var modState in States)
                modState.OnBeginEncounter();
        }

        public void OnEndEncounter()
        {
            foreach (var state in States)
                state.OnEndEncounter();
        }
    }
}
