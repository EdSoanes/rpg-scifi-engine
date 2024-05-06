using Newtonsoft.Json;
using Rpg.Sys.Actions;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Components
{
    public interface IState
    {
        Guid Id { get; }
        string Name { get; }
        int Effect { get; }
        ModifierDuration Duration { get; }
        ActionCost ActivateCost { get; }
        ActionCost DeactivateCost { get; }
        bool IsActive { get; set; }
        Condition OnActive(Actor actor);
        Condition OnInactive(Actor actor);
        string[]? StatesPermittedWhenActive();
        string ConditionName();
    }

    public abstract class State<T> : ModObject, IState
        where T : Artifact
    {
        [JsonIgnore] private T? Artifact { get; set; }
        [JsonProperty] private Guid ArtifactId { get; set; }

        [JsonProperty] public int Effect {  get; private set; }
        [JsonProperty] public ModifierDuration Duration { get; private set; } = new ModifierDuration();

        [JsonProperty] public ActionCost ActivateCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost DeactivateCost { get; private set; } = new ActionCost();

        [JsonConstructor] private State() { }

        public State(Guid artifactId, string stateName)
        {
            ArtifactId = artifactId;
            Name = stateName;
        }

        public bool IsActive { get; set; }
        public bool IsSingleton { get; set; }

        public Condition OnActive(Actor actor) 
            => OnActive(actor, Artifact);

        public Condition OnInactive(Actor actor)
            => OnInactive(actor, Artifact);

        public virtual string[]? StatesPermittedWhenActive() 
            => null;

        protected virtual Condition OnActive(Actor actor, T artifact) 
            => new Condition(artifact.Id, ConditionName(), Duration);

        protected virtual Condition OnInactive(Actor actor, T artifact) 
            => new Condition(artifact.Id, ConditionName(), Duration);

        protected override void OnInitialize()
        {
            var artifact = Graph.Current.Get.Entity<T>(ArtifactId);
            if (artifact != null)
                Artifact = artifact;
        }

        public string ConditionName()
        {
            var name = $"{Name}.{(IsActive ? "Active" : "Inactive")}";
            return IsSingleton
                ? Name
                : $"{Id}.{name}";
        }
    }
}
