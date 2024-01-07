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
        Modifier[] Effects(Actor actor);
        string[]? StatesPermittedWhenActive();
    }

    public abstract class State<T> : ModdableObject, IState
        where T : Artifact
    {
        [JsonIgnore] private T? Artifact { get; set; }
        [JsonProperty] private Guid ArtifactId { get; set; }

        [JsonProperty] public int Effect {  get; private set; }
        [JsonProperty] public int DurationTurns { get; private set; } = 1;

        [JsonProperty] public ActionCost ActivateCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost DeactivateCost { get; private set; } = new ActionCost();

        [JsonConstructor] private State() { }

        public State(Guid artifactId, string stateName)
        {
            ArtifactId = artifactId;
            Name = stateName;
        }

        public bool IsActive { get; set; }

        public Modifier[] Effects(Actor actor) => Effects(actor, Artifact);

        public virtual string[]? StatesPermittedWhenActive() => null;
        protected virtual Modifier[] Effects(Actor actor, T artifact) => new Modifier[0];

        public override Modifier[] OnSetup()
        {
            var mods = base.OnSetup();
             
            var artifact = Graph.Entities.Get(ArtifactId) as T;
            if (artifact != null)
                Artifact = artifact;

            return mods;
        }
    }
}
