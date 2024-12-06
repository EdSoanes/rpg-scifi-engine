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

        public bool OverrideBaseValue(RpgGraph graph, OverrideBaseValue overrideBaseValue)
        {
            var entity = graph.GetObject(overrideBaseValue.PropRef.EntityId);
            var res = entity?.OverrideBaseValue(overrideBaseValue.PropRef.Prop, overrideBaseValue.OverrideValue) ?? false;

            graph.Time.TriggerEvent();

            return res;
        }
    }
}
