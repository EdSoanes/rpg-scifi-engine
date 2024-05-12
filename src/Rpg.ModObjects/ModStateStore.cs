using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModStateStore : ITemporal
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public ModObject? Entity { get; set; }
        [JsonProperty] public List<ModState> States { get; private set; } = new List<ModState>();

        public ModStateStore() { }

        public void Add(ModState modState)
        {
            if (!States.Contains(modState))
                States.Add(modState);
        }

        public ModState[] All()
            => States.ToArray();

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            Entity = entity;
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
