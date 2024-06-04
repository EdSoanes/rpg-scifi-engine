using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Time;

namespace Rpg.Sys.Components.Values
{
    public class MaxCurrentValue : RpgEntityComponent
    {
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(string entityId, string name, int max)
            : base(entityId, name)
        {
            Max = max;
        }

        public MaxCurrentValue(string entityId, string name, int max, int current)
            : base(entityId, name)
        {
            Max = max;
            Current = current;
        }

        protected override void OnCreating()
        {
            this.BaseMod(x => x.Current, x => x.Max);
        }
    }
}
