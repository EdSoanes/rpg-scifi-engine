using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class MaxCurrentValue : ModdableObject
    {
        [JsonProperty] public int Max { get; private set; }
        [JsonProperty] public int Current { get; private set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(int max, int current) 
        {
            Max = max;
            Current = current;
        }
    }
}
