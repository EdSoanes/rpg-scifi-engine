using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using System.Text.Json.Serialization;

namespace Rpg.Sys.Components.Values
{
    public class HealthValue : RpgComponent
    {
        [JsonInclude]
        [MinZero]
        public int Max { get; protected set; }

        [JsonInclude]
        [MinZero(Ignore = true)]
        public int Current { get; protected set; }

        [JsonConstructor] private HealthValue() { }

        public HealthValue(string name)
            : base(name)
        {
        }
    }
}
