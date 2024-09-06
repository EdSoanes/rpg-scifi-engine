using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using System.Text.Json.Serialization;

namespace Rpg.Sys.Components.Values
{
    public class DefenseValue : RpgComponent
    {
        [JsonInclude] 
        [Percent]
        public int Value { get; protected set; }

        [JsonInclude]
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
