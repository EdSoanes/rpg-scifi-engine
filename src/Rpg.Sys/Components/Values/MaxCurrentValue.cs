using Newtonsoft.Json;
using Rpg.Sys.Moddable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
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
