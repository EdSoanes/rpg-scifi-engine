using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Container : ModdableObject
    {
        public const string TurnAction = "TurnAction";
        public const string LeftHand = "LeftHand";
        public const string RightHand = "RightHand";
        public const string Equipment = "Equipment";
        public const string Environment = "Environment";

        private readonly int _maxCapacity;
        private readonly int _maxArtifacts;

        public Container(int maxCapacity = int.MaxValue, int maxArtifacts = int.MaxValue)
        {
            _maxCapacity = maxCapacity;
            _maxArtifacts = maxArtifacts;
        }

        [JsonProperty] public ActionCost AddCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost RemoveCost { get; private set; } = new ActionCost();

        [JsonProperty] protected List<Artifact> Artifacts { get; set; } = new List<Artifact>();

        [Moddable] public int MaxCapacity { get => Resolve(); }
        [Moddable] public int MaxArtifacts { get => Resolve(); }
        public int Encumbrance { get => Artifacts.Sum(x => x.Weight); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _maxCapacity, x => x.MaxCapacity),
                BaseModifier.Create(this, _maxArtifacts, x => x.MaxArtifacts)
            };
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

            Graph?.Mods?.NotifyPropertyChanged(Id, nameof(Encumbrance));
        }

        public Artifact? Get(Guid id) => Artifacts.FirstOrDefault(x => x.Id == id);

        public Artifact? Remove(Guid id)
        {
            if (!Artifacts.Any(x => x.Id == id))
                throw new ArgumentException($"Artifact {id} does not exist in container");
            
            var toRemove = Artifacts.Single(x => x.Id == id);

            Artifacts.Remove(toRemove);
            toRemove.ContainerId = null;

            Graph!.Mods!.NotifyPropertyChanged(Id, nameof(Encumbrance));

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
