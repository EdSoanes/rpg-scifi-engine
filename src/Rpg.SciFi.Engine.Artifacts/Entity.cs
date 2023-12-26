using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Xml.Linq;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity
    {
        [JsonIgnore] protected IContext Context { get; set; }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public MetaEntity MetaData { get; private set; }
        [JsonProperty] protected Dictionary<string, Container> Containers { get; private set; } = new Dictionary<string, Container>();

        public Entity()
        {
            Name = GetType().Name;
            MetaData = this.CreateMetaEntity();
        }

        public Dice Evaluate(string prop) => Context?.Evaluate(Id, prop) ?? 0;

        public int Resolve(string prop) => Context?.Resolve(Id, prop) ?? 0;

        public string[] Describe(string prop) => Context?.Describe(this, prop, true) ?? new string[0];
        public string[] Describe(ModdableProperty? moddableProperty) => Context?.Describe(moddableProperty, true) ?? new string[0];

        public bool AddContainer(Container container)
        {
            if (!Containers.ContainsKey(container.Name))
            {
                Containers.Add(container.Name, container);
                return true;
            }

            return false;
        }

        public Container? RemoveContainer(string containerName)
        {
            if (Containers.ContainsKey(containerName))
            {
                var removed = Containers[containerName];
                Containers.Remove(containerName);
                return removed;
            }

            return null;
        }

        public Container? GetContainer(string container) => Containers.ContainsKey(container) ? Containers[container] : null;
        public bool AddArtifact(string containerName, Artifact artifact)
        {
            var container = GetContainer(containerName);
            if (container != null && container.Get(artifact.Id) == null)
            {
                container.Add(artifact);
                return true;
            }

            return false;
        }

        public Artifact? GetArtifact(string container, Guid id) => GetContainer(container)?.Get(id);
        public Artifact? RemoveArtifact(string container, Guid id) => GetContainer(container)?.Remove(id);
    }
}
