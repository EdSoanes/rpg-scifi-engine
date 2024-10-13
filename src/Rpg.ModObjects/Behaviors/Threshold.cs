using Newtonsoft.Json;

namespace Rpg.ModObjects.Behaviors
{
    public class Threshold : BaseBehavior
    {
        [JsonProperty] public int Min { get; protected set; }
        [JsonProperty] public int Max { get; protected set; }

        [JsonConstructor] private Threshold() { }

        public Threshold(int min, int max)
            : base()
        {
            Min = min;
            Max = max;
        }
    }
}
