using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Expire : Operation
    {
        public Expire(Graph graph) 
            : base(graph) { }

        public override void Execute(params Condition[] conditions)
        {
            foreach (var condition in conditions)
            {
                condition.Duration.Expire(Graph.Turn);
                Execute(condition.GetModifiers());
            }
        }

        public override void Execute(params Modifier[] mods) 
        {
            var updates = new List<ModProp>();
            foreach (var mod in mods)
            {
                var updated = Graph.Mods.Iterate(mod, (modProp) =>
                {
                    mod.Duration.Expire(Graph!.Turn);
                    return modProp;
                });

                AddPropertyChanged(updated);
            }

            NotifyPropertyChanged();
        }
    }
}
