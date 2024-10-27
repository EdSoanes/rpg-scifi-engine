using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods
{
    public class ModSetDescription
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public Dictionary<string, Dice> Values { get; private set; } = new();

        [JsonConstructor] private ModSetDescription() { }

        public ModSetDescription(string name) : base() 
        {
            Name = name;
        }

        public void Set(PropRef propRef, Dice value)
            => Values.Add(propRef.ToString(), value);

        public Dice Get(PropRef propRef)
            => Values.ContainsKey(propRef.ToString())
                ? Values[propRef.ToString()] 
                : null;
    }
}
