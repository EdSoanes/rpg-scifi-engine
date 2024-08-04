using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Props;

namespace Rpg.ModObjects.Meta
{
    public interface IMetaSystem
    {
        string Identifier { get; }
        string[]? Namespaces { get; set; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
        MetaObj[] Objects { get; set; }
        MetaAction[] Actions { get; set; }
        MetaState[] States { get; set; }
        MetaPropAttribute[] PropUIs { get; set; }
        ActionGroup[] ActionGroups { get; set; }
        MetaObj AsContentTemplate(MetaObj obj);
    }
}
