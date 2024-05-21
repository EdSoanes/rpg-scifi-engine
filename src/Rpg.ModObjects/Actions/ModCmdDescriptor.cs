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
        public string CmdName { get; set; }
        public ModCmdArg[] Args { get; set; } = new ModCmdArg[0];

        public ModCmdDescriptor(Guid entityId, string cmdName)
        {
            EntityId = entityId;
            CmdName = cmdName;
        }

        public ModAction Run(ModGraph graph)
        {
            var entity = graph.GetEntity(EntityId);
            return entity.ExecuteFunction<ModAction>(CmdName);
        }
    }
}
