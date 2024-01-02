using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity : ModdableObject
    {
        [JsonProperty] protected Dictionary<string, Container> Containers { get; private set; } = new Dictionary<string, Container>();

        public Entity()
        {
            Name = GetType().Name;
        }

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
        public bool HasArtifacts(string containerName)
        {
            var container = GetContainer(containerName);
            return container?.Any() ?? false;
        }
    }
}
