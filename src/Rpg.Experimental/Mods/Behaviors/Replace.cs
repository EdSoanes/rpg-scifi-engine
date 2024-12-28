using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods.Behaviors
{
    public class Replace : IModBehavior
    {
        public void OnAdding(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            if (!propertyData.Mods.Any(x => x.Id == mod.Id))
            {
                mod.OnTimeEvent(graph);

                var replaceMods = ModFilters.Active(propertyData.Mods)
                    .Where(x => x.ModBehavior is Replace && (x.ModType == ModType.Standard || x.ModType == ModType.Synced))
                    .ToList();

                foreach (var replaceMod in replaceMods)
                    replaceMod.Expire(graph);

                propertyData.Mods.Add(mod);
                graph.ChangeTracker.PropUpdated(propertyData.ObjectId, propertyData.Prop);
            }
        }

        public void OnAfterTimeEvent(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            if (mod.Expiry == Time.LifecycleExpiry.Active && graph.Time.Now.IsAfterEncounterTime && (mod.Start != TimePointType.TimeBegins || mod.End != TimePointType.TimeEnds))
                mod.Lifespan(TimePointType.TimeBegins, TimePointType.TimeEnds);
        }

        public void OnBeforeTimeEvent(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            
        }
    }
}
