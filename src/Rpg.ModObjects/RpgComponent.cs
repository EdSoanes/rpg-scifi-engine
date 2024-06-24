using Newtonsoft.Json;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public abstract class RpgComponent : RpgObject
    {
        [JsonProperty] 
        [TextUI(Ignore = true)]
        public string EntityId { get; private set; }

        [JsonConstructor] protected RpgComponent() { }

        public RpgComponent(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}
