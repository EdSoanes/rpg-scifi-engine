using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;

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

        public Dice Evaluate(string prop) => Context?.Mods.Evaluate(Id, prop) ?? 0;

        public int Resolve(string prop) => Context?.Mods.Resolve(Id, prop) ?? 0;

        public string[] Describe(string prop) => Context?.Mods.Describe(this, prop) ?? new string[0];
    }
}
