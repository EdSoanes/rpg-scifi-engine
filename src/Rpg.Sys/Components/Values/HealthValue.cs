using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Meta.Props;
using Rpg.Sys.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class HealthValue : RpgComponent
    {
        [JsonProperty]
        [MinZero]
        public int Max { get; protected set; }

        [JsonProperty]
        [MinZero(Ignore = true)]
        public int Current { get; protected set; }

        [JsonConstructor] private HealthValue() { }

        public HealthValue(string entityId, string name)
            : base(entityId, name)
        {
        }
    }
}
