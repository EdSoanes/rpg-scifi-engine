using Newtonsoft.Json;

namespace Rpg.ModObjects.Tests.Models
{
    public class MaxCurrentValue : ModObject
    {
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(string name, int max, int current) 
        {
            Name = name;
            Max = max;
            Current = current;
        }
    }
}
