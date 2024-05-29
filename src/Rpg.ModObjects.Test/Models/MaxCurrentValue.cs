using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using System.Diagnostics;

namespace Rpg.ModObjects.Tests.Models
{
    public class MaxCurrentValue : RpgObject
    {
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(string name, int max) 
        {
            Name = name;
            Max = max;
        }

        protected override void OnCreate()
        {
            this.AddMod(new Base(), x => x.Current, x => x.Max);
        }
    }
}
