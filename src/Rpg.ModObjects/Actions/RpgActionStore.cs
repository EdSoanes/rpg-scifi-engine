using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Actions
{
    public class RpgActionStore : ModBaseStore<string, RpgAction>
    {
        public RpgActionStore(string entityId)
            : base(entityId) { }

        public void Add(params RpgAction[] modCmds)
        {
            foreach (var modCmd in modCmds)
            {
                if (!Contains(modCmd))
                {
                    Items.Add(modCmd.InstanceName, modCmd);
                    if (Graph != null)
                    {
                        var entity = Graph.GetEntity(EntityId!);
                        modCmd.OnGraphCreating(Graph!, entity!);
                    }
                }
            }
        }

        public override void OnGraphCreating(RpgGraph graph, RpgObject entity)
        {
            base.OnGraphCreating(graph, entity);
            foreach (var cmd in Get())
                cmd.OnGraphCreating(graph, entity);
        }

        public override void OnUpdating(RpgGraph graph, TimePoint time)
        {
            base.OnUpdating(graph, time);
        }
    }
}
