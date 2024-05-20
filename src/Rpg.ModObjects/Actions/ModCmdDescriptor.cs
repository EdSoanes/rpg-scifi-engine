using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ModCmdDescriptor
    {
        public Guid EntityId { get; set; }
        public string ActionName { get; set; }
        public ModCmdArg[] Args { get; set; } = new ModCmdArg[0];

        public ModCmdDescriptor(Guid entityId, string actionName)
        {
            EntityId = entityId;
            ActionName = actionName;
        }
    }
}
