using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects.Cmds
{
    public class ModCmdStore : ModBaseStore<string, ModCmd>
    {
        public ModCmdStore(string entityId)
            : base(entityId) { }

        public void Add(params ModCmd[] modCmds)
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

        public override void OnUpdating(RpgGraph graph)
        {
            base.OnUpdating(graph);
        }
    }
}
