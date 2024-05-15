using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ModObjectActionDescriptor
    {
        public Guid EntityId { get; set; }
        public string ActionName { get; set; }

        public ModObjectActionDescriptor(Guid entityId, string actionName)
        {
            EntityId = entityId;
            ActionName = actionName;
        }
    }
}
