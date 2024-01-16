using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Remove : Operation
    {
        public Remove(Graph graph) 
            : base(graph) { }

        public override void Execute(params ModdableObject[] entities)
        {
            var modProps = entities.SelectMany(x => Graph.Mods.Get(x.Id)).ToArray();
            Graph.Mods.Remove(modProps);

            AddPropertyChanged(modProps);
            NotifyPropertyChanged();

            var entityIds = entities.Select(x => x.Id).ToList();
            var conditions = Graph.Conditions.Where(x => x.OwningEntityId != null && entityIds.Contains(x.OwningEntityId.Value));
            Execute(conditions.ToArray());

            Graph.Entities.Remove(entities);
        }

        public override void Execute(params Condition[] conditions)
        {
            var mods = conditions.SelectMany(x => x.GetModifiers()).ToArray();
            Execute(mods);

            foreach (var condition in conditions)
                Graph.Conditions.Remove(condition);
        }

        public override void Execute(params Modifier[] mods) 
        {
            foreach (var mod in mods)
            {
                var modified = Graph.Mods.Iterate(mod, (modProp) =>
                {
                    var removed = modProp.Remove(mod);
                    if (removed != null)
                        AddPropertyChanged(modProp);

                    return removed != null;
                });
            }

            NotifyPropertyChanged();
        }
    }
}
