using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

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
            Min = min;
            Max = max;
        }
    }
}
