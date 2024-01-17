using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Expire : Operation
    {
        public Expire(Graph graph) 
            : base(graph) { }

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
                var updated = Graph.Mods.Iterate(mod, (modProp) =>
                {
                    mod.Duration.Expire(Graph!.Turn);
                    return modProp;
                });

                Graph.NotifyOp.Queue(updated);
            }

            Graph.NotifyOp.Send();
        }
    }
}
