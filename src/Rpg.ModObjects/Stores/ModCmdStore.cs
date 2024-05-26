using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Stores
{
    public class ModCmdStore : ModBaseStore<string, ModCmd>
    {
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

        public override void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            base.OnGraphCreating(graph, entity);
            foreach (var cmd in Get())
                cmd.OnGraphCreating(graph, entity);
        }

        public override void OnBeginEncounter()
        {

        }

        public override void OnEndEncounter()
        {

        }

        public override void OnTurnChanged(int turn)
        {

        }
    }
}
