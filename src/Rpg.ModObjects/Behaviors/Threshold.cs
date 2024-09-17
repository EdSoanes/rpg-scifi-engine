using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Behaviors
{
    public class Threshold : BaseBehavior
    {
        [JsonInclude] public int Min { get; protected set; }
        [JsonInclude] public int Max { get; protected set; }

        [JsonConstructor] private Threshold() { }

        public Threshold(int min, int max)
            : base()
        {
            Min = min;
            Max = max;
        }
    }
}
