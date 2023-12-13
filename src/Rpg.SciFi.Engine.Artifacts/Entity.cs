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

        public Entity()
        {
            Name = GetType().Name;
            MetaData = this.CreateMetaEntity("");
        }

        public Dice Evaluate(string prop) => Meta.Mods.Evaluate(Id, prop);

        public int Resolve(string prop) => Meta.Mods.Resolve(Id, prop);

        public string[] Describe(string prop) => Meta.Mods.Describe(this, prop);
    }
}
