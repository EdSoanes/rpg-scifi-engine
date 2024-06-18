using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using System.Xml.Linq;

namespace Rpg.Sys.Components.Values
{
    public class DefenseValue : RpgComponent
    {
        [JsonProperty] 
        [PercentUI]
        public int Value { get; protected set; }

        [JsonProperty]
        [PercentUI]
        public int Shielding { get; protected set; }

        [JsonConstructor] private DefenseValue() { }

        public DefenseValue(string entityId, string name, int value, int shielding)
            : base(entityId, name)
        {
            Value = value;
            Shielding = shielding;
        }
    }
}
