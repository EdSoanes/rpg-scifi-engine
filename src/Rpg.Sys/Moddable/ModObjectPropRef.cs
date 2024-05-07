using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Moddable
{
    public class ModObjectPropRef
    {
        [JsonProperty] public Guid EntityId { get; protected set; }
        [JsonProperty] public string Prop { get; protected set; }

        [JsonConstructor] private ModObjectPropRef() { }

        public ModObjectPropRef(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public static bool operator ==(ModObjectPropRef d1, ModObjectPropRef d2)
            => d1.EntityId == d2.EntityId && d1.Prop == d2.Prop;
        
        public static bool operator !=(ModObjectPropRef d1, ModObjectPropRef d2) 
            => d1.EntityId != d2.EntityId || d1.Prop != d2.Prop;

    }
}
