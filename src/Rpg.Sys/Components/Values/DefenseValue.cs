using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Newtonsoft.Json;

namespace Rpg.Sys.Components.Values
{
    public class DefenseValue : RpgComponent
    {
        [JsonProperty] 
        [Percent]
        public int Value { get; protected set; }

        [JsonProperty]
        [Percent]
        public int Shielding { get; protected set; }

        [JsonConstructor] private DefenseValue() { }

        public DefenseValue(string name, int value, int shielding)
            : base(name)
        {
            Value = value;
            Shielding = shielding;
        }
    }
}
