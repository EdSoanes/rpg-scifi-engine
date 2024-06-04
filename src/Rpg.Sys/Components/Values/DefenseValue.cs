using Newtonsoft.Json;
using Rpg.ModObjects;
using System.Xml.Linq;

namespace Rpg.Sys.Components.Values
{
    public class DefenseValue : RpgEntityComponent
    {
        [JsonProperty] public int Value { get; protected set; }
        [JsonProperty] public int Shielding { get; protected set; }

        [JsonConstructor] private DefenseValue() { }

        public DefenseValue(string entityId, string name, int value, int shielding)
            : base(entityId, name)
        {
            Value = value;
            Shielding = shielding;
        }
    }
}
