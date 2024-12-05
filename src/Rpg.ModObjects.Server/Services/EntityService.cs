using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public class EntityService
    {
        private readonly GraphService _graphService;

        public EntityService(GraphService graphService)
        {
            _graphService = graphService;
        }

        public RpgGraphState OverrideBaseValue(RpgGraph graph, OverrideBaseValue overrideBaseValue)
        {
            var entity = graph.GetObject(overrideBaseValue.PropRef.EntityId);
            var originalValue = graph.CalculateOriginalBasePropValue(overrideBaseValue.PropRef);
            if (entity != null && originalValue != null)
            {
                entity.RemoveMods(entity.GetMods(overrideBaseValue.PropRef.Prop, mod => mod.IsOverrideMod()));

                if (originalValue.Value.Roll() != overrideBaseValue.Value)
                    entity.AddMod(new Override(), overrideBaseValue.PropRef, overrideBaseValue.Value);

                graph.Time.TriggerEvent();
            }

            return graph.GetGraphState();
        }
    }
}
