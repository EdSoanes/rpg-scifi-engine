using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods.Behaviors
{
    public interface IModBehavior
    {
        void OnAdding(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData);
        void OnBeforeTimeEvent(Mod mod, RpgGraph graph);
        void OnAfterTimeEvent(Mod mod, RpgGraph graph);
    }
}
