using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Behaviors
{
    public class Threshold : BaseBehavior
    {
        [JsonProperty] public int Min { get; private set; }
        [JsonProperty] public int Max { get; private set; }

        [JsonConstructor] private Threshold() { }

        public Threshold(int min, int max)
            : base()
        {
            Type = ModType.Initial;
            Min = min;
            Max = max;
        }
    }
}
