using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Tests.Models
{
    public class MaxCurrentValue : RpgComponent
    {
        [JsonInclude] public int Max { get; protected set; }
        [JsonInclude] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(string name, int max)
            : base(name)
        {
            Max = max;
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            this.AddMod(new Base(), x => x.Current, x => x.Max);
        }
    }
}
