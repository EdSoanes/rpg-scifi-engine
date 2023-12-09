using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public MetaEntity Meta { get; private set; }

        public Entity()
        {
            Name = GetType().Name;
            Meta = this.CreateMetaEntity("");
        }

        public Dice Evaluate(string prop)
        {
            Dice dice = Dice.Sum(Meta.Mods.Get(prop).Select(x => x.Evaluate()));
            return dice;
        }

        public int Resolve(string prop)
        {
            //TODO: We need to store the result so we don't get different resolutions each time this is called.
            Dice dice = Evaluate(prop);
            return dice.Roll();
        }

        public virtual string[] Describe(string prop)
        {
            var res = new List<string> { $"{Name}.{prop} => {Evaluate(prop)}" };

            var description = Meta.Mods.Describe(prop)
                ?? new string[0];

            return description;
        }
    }
}
