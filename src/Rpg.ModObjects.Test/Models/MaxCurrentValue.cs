using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Tests.Models
{
    public class MaxCurrentValue : RpgEntityComponent
    {
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(Guid entityId, string name, int max)
            : base(entityId, name)
        {
            Max = max;
        }

        protected override void OnCreating()
        {
            this.AddMod(new Base(), x => x.Current, x => x.Max);
        }
    }
}
