using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class State
    {
        [JsonConstructor] private State() { }

        public State(string name, params Modifier[] modifiers)
        {
            Name = name;
            Modifiers = modifiers.Where(x => x.ModifierType == ModifierType.State).ToArray();
        }

        [JsonProperty] public virtual string Name { get; set; } = string.Empty;
        [JsonProperty] public virtual Modifier[] Modifiers { get; set; } = new Modifier[0];
    }

    public sealed class States
    {
        [JsonProperty] private State[] _states { get; set; } = new State[0];
        [JsonProperty] private List<State> _activeStates { get; set; } = new List<State>();

        public States() { }
        public States(params State[] states)
        {
            _states = states;
        }

        public bool HasState(string stateName)
        {
            return _states.Any(x => x.Name == stateName);
        }

        public void Activate(string stateName)
        {
            var state = _states.FirstOrDefault(x => x.Name == stateName);
            if (state != null && !_activeStates.Contains(state))
                _activeStates.Add(state);
        }

        public void Deactivate(string stateName)
        {
            var states = _activeStates.Where(x => x.Name == stateName);
            foreach (var state in states)
                _activeStates.Remove(state);
        }
    }
}
