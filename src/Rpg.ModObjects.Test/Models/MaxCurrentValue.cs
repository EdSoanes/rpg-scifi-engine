using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Models
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

        protected override void OnCreating()
        {
            this.BaseMod(x => x.Current, x => x.Max);
        }
    }
}
