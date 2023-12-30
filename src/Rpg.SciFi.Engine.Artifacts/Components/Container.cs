using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Container : Entity
    {
        public const string TurnAction = "TurnAction";
        public const string LeftHand = "LeftHand";
        public const string RightHand = "RightHand";
        public const string Equipment = "Equipment";
        public const string Environment = "Environment";

        private readonly int _maxCapacity;
        public Container(int maxCapacity = int.MaxValue)
        {
            _maxCapacity = maxCapacity;
        }

        [JsonProperty] protected List<Artifact> Artifacts { get; set; } = new List<Artifact>();

        [Moddable] public int MaxCapacity { get => this.Resolve(nameof(MaxCapacity)); }
        public int Encumbrance { get => Artifacts.Sum(x => x.Weight); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _maxCapacity, x => x.MaxCapacity)
            };
        }

        [Ability()]
        [Input(Param = "actor", BindsTo = "Actor")]
        [Input(InputSource = InputSource.Player, Param = "artifact")]
        public Turns.Action Get(Actor actor, Artifact artifact)
        {
            var action = actor.Act(this, nameof(Get), 1, 1, 1);
            action.AddArtifact(TurnAction, artifact);
            return action;
        }

        [Ability()]
        [Input(Param = "actor", BindsTo = "Actor")]
        [Input(InputSource = InputSource.Player, Param = "artifact")]
        public Turns.Action Put(Actor actor, Artifact artifact)
        {
            var action = actor.Act(this, nameof(Put), 1, 1, 1);
            action.AddArtifact(TurnAction, artifact);
            return action;
        }

        public void Add(Artifact artifact)
        {
            if (artifact.ContainerId != null)
                throw new ArgumentException($"Artifact {artifact.Id} {artifact.Name} exists in container {artifact.ContainerId}");

            if (Artifacts.Any(x => x.Id == artifact.Id))
                throw new ArgumentException($"Artifact {artifact.Id} {artifact.Name} already exists in this container");

            if (Artifacts.Sum(x => x.Weight) + artifact.Weight > MaxCapacity)
                throw new ArgumentException($"Adding artifact {artifact.Id} {artifact.Name} would exceed capacity");

            artifact.ContainerId = Id;
            Artifacts.Add(artifact);
        }

        public Artifact? Get(Guid id) => Artifacts.FirstOrDefault(x => x.Id == id);

        public Artifact? Remove(Guid id)
        {
            if (!Artifacts.Any(x => x.Id == id))
                throw new ArgumentException($"Artifact {id} does not exist in container");
            
            var toRemove = Artifacts.Single(x => x.Id == id);

            Artifacts.Remove(toRemove);
            toRemove.ContainerId = null;

            return toRemove;
        }

        public bool Has(Guid id) => Get(id) != null;

        public bool Any() => Artifacts.Any();

        public void Clear()
        {
            Artifacts.Clear();
        }
    }
}
