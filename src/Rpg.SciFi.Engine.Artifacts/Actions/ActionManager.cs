using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Actions
{
    public class ActionManager
    {
        private EntityGraph? _graph;
        
        [JsonProperty] public List<TurnAction> Actions { get; private set; } = new List<TurnAction>();

        public void Initialize(EntityGraph graph)
        {
            _graph = graph;
        }

        public int Current { get; private set; }

        public void StartEncounter()
        {
            Current = 1;
            _graph!.Mods!.Remove(Current);
        }
        
        public void Increment()
        {
            Current++;
            _graph!.Mods!.Remove(Current);
        }

        public void SetTurn(int turn)
        {
            Current = turn;
            _graph!.Mods!.Remove(Current);
        }

        public void EndEncounter()
        {
            Current = 0;
            _graph!.Mods!.Remove(Current);
        }
    }
}
