using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Description
{
    public class PropInfo
    {
        [JsonProperty] public string EntityId { get; internal init; }
        [JsonProperty] public string Name { get; internal init; }
        [JsonProperty] public string Archetype { get; internal init; }
        [JsonProperty] public string Prop { get; internal init; }
        [JsonProperty] public Dice Value { get; internal set; }

        [JsonProperty] public List<ModInfo> Mods { get; private set; } = new();

        [JsonConstructor] protected PropInfo() { }

        internal PropInfo(RpgObject rpgObj, string prop)
        {
            EntityId = rpgObj.Id;
            Name = rpgObj.Name;
            Archetype = rpgObj.Archetype;
            Prop = prop;
        }

        public override string ToString()
        {
            return $"{Value} <= {Archetype}.{Prop}";
        }
    }
}
