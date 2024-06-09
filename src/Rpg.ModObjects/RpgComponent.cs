using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public abstract class RpgComponent : RpgObject
    {
        [JsonProperty] public string EntityId { get; private set; }

        [JsonConstructor] protected RpgComponent() { }

        public RpgComponent(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}
