using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Runtime.CompilerServices;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity : System.ComponentModel.INotifyPropertyChanged
    {
        protected ModStore? _modStore;
        protected IPropEvaluator? _propEvaluator;
        protected TurnManager? _turnManager;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public MetaEntity MetaData { get; private set; }
        [JsonProperty] protected Dictionary<string, Container> Containers { get; private set; } = new Dictionary<string, Container>();

        public Entity()
        {
            Name = GetType().Name;
            MetaData = this.CreateMetaEntity();
        }

        public void Initialize(ModStore modStore, IPropEvaluator propEvaluator, TurnManager turnManager)
        {
            _modStore = modStore;
            _propEvaluator = propEvaluator;
            _turnManager = turnManager;
        }

        internal void PropChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
        }

        public Dice Evaluate([CallerMemberName] string prop = "") => _propEvaluator?.Evaluate(Id, prop) ?? 0;

        public int Resolve([CallerMemberName] string prop = "") => _propEvaluator?.Evaluate(Id, prop).Roll() ?? 0;

        public string[] Describe(string prop) => _propEvaluator?.Describe(Id, prop, true) ?? new string[0];

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
