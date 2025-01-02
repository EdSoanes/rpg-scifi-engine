using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods.Behaviors
{
    public class Combine : IModBehavior
    {
        public void OnAdding(Mod mod, RpgGraph graph, RpgPropertyDataModdable propertyData)
        {
            if (!propertyData.Mods.Any(x => x.Id == mod.Id))
            {
                var modValue = ModCalculator.Value(graph, mod);
                var combineMods = ModFilters.Active(propertyData.Mods)
                    .Where(x => x.ModBehavior is Combine && x.ModType == mod.ModType)
                    .ToList();

                var oldVal = ModCalculator.Value(graph, combineMods);
                var val = oldVal == null
                    ? modValue
                    : modValue + oldVal;

                mod.SetSource(val ?? Dice.Zero);

                foreach (var combineMod in combineMods.Where(x => x.Id != mod.Id))
                    combineMod.Expire(graph);

                propertyData.Mods.Add(mod);
                graph.ChangeTracker.PropUpdated(propertyData.ObjectId, propertyData.Prop);
            }
        }

        public void OnAfterTimeEvent(Mod mod, RpgGraph graph)
        {
            if (mod.Expiry == Time.LifecycleExpiry.Active && graph.Time.Now.IsAfterEncounterTime && (mod.Start != TimePointType.TimeBegins || mod.End != TimePointType.TimeEnds))
                mod.Lifespan(TimePointType.TimeBegins, TimePointType.TimeEnds);
        }

        public void OnBeforeTimeEvent(Mod mod, RpgGraph graph)
        {
            
        }
    }
}
