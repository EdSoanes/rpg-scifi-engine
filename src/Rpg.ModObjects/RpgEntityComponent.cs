using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public abstract class RpgEntityComponent : RpgObject
    {
        [JsonProperty] public string EntityId { get; private set; }

        [JsonConstructor] protected RpgEntityComponent() { }

        public RpgEntityComponent(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}
