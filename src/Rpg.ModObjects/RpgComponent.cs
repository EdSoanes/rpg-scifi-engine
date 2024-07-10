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
        public string EntityId { get; private set; }

        [JsonConstructor] protected RpgComponent() { }

        public RpgComponent(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            EntityId ??= entity!.Id;
        }
    }
}
