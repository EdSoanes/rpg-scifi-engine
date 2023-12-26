using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public MetaEntity MetaData { get; private set; }
        [JsonIgnore] protected IContext Context { get; set; }

        public Entity()
        {
            Name = GetType().Name;
            MetaData = this.CreateMetaEntity();
        }

        public Dice Evaluate(string prop) => Context?.Evaluate(Id, prop) ?? 0;

        public int Resolve(string prop) => Context?.Resolve(Id, prop) ?? 0;

        public string[] Describe(string prop) => Context?.Describe(this, prop, true) ?? new string[0];
        public string[] Describe(ModdableProperty? moddableProperty) => Context?.Describe(moddableProperty, true) ?? new string[0];
    }
}
