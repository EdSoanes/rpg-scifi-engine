using Newtonsoft.Json;
using Rpg.Sys.Actions;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Components
{
    public interface IState
    {
        Guid Id { get; }
        string Name { get; }
        int Effect { get; }
        int DurationTurns { get; }
        ActionCost ActivateCost { get; }
        ActionCost DeactivateCost { get; }
        bool IsActive { get; set; }
        Modifier[] Effects();
        string[]? StatesPermittedWhenActive();
    }

    public abstract class State<T> : ModdableObject, IState
        where T : Artifact
    {
        [JsonIgnore] internal T? Parent { get; private set; }

        [JsonProperty] public int Effect {  get; private set; }
        [JsonProperty] public int DurationTurns { get; private set; } = 1;

        [JsonProperty] public ActionCost ActivateCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost DeactivateCost { get; private set; } = new ActionCost();

        public bool IsActive { get; set; }

        public Modifier[] Effects() => Effects(Parent);

        public virtual string[]? StatesPermittedWhenActive() => null;
        protected virtual Modifier[] Effects(T? parent) => new Modifier[0];
    }
}
