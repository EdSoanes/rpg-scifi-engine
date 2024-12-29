using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods.Behaviors
{
    public class Standard : IModBehavior
    {
        public void OnAdding(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            if (!propertyData.Mods.Any(x => x.Id == mod.Id))
            {
                propertyData.Mods.Add(mod);
                graph.ChangeTracker.PropUpdated(propertyData.ObjectId, propertyData.Prop);
            }
        }

        public void OnAfterTimeEvent(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            
        }

        public void OnBeforeTimeEvent(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            
        }
    }
}
