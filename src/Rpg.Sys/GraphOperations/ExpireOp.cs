using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class ExpireOp : Operation
    {
        public ExpireOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public void Conditions(params Condition[] conditions)
        {
            foreach (var condition in conditions)
            {
                condition.Duration.Expire(Graph.Turn);
                Mods(condition.GetModifiers());
            }
        }

        public void Mods(params Modifier[] mods) 
        {
            var updates = new List<ModProp>();
            foreach (var mod in mods)
            {
                var updated = ModStore.Iterate(mod, (modProp) =>
                {
                    mod.Duration.Expire(Graph!.Turn);
                    return modProp;
                });

                Graph.Notify.Queue(updated);
            }

            Graph.Notify.Send();
        }
    }
}
