using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Container : Entity
    {
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

        public void Add(Artifact artifact)
        {
            if (Artifacts.Any(x => x.Id == artifact.Id))
                throw new ArgumentException($"Artifact {artifact.Id} {artifact.Name} exists in container");

            if (Artifacts.Sum(x => x.Weight) + artifact.Weight > MaxCapacity)
                throw new ArgumentException($"Adding artifact {artifact.Id} {artifact.Name} would exceed capacity");

            Artifacts.Add(artifact);
        }

        public Artifact? Get(Guid id) => Artifacts.FirstOrDefault(x => x.Id == id);

        public void Remove(Guid id)
        {
            if (!Artifacts.Any(x => x.Id == id))
                throw new ArgumentException($"Artifact {id} does not exist in container");
            
            var toRemove = Artifacts.Single(x => x.Id == id);
            Artifacts.Remove(toRemove);
        }

        public void Clear()
        {
            Artifacts.Clear();
        }
    }
}
