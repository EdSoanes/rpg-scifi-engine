using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Newtonsoft.Json;

namespace Rpg.Sys.Components.Values
{
    public class HealthValue : RpgComponent
    {
        [JsonProperty]
        [MinZero]
        public int Max { get; protected set; }

        [JsonProperty]
        [MinZero(Ignore = true)]
        public int Current { get; protected set; }

        [JsonConstructor] private HealthValue() { }

        public HealthValue(string name)
            : base(name)
        {
        }
    }
}
