using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Remove : Operation
    {
        public Remove(Graph graph) 
            : base(graph) { }

        public void Entities(params ModdableObject[] entities)
        {
            var moddableObjects = new List<ModdableObject>();
            foreach (var entity in entities)
            {
                var modObjs = Descendants(entity);
                moddableObjects.AddRange(modObjs);
            }

            var modProps = moddableObjects.SelectMany(x => Graph.Mods.Get(x.Id)).ToArray();
            Graph.Mods.Remove(modProps);

            Graph.NotifyOp.Queue(modProps);
            Graph.NotifyOp.Send();

            var entityIds = entities.Select(x => x.Id).ToList();
            var conditions = Graph.Conditions.Where(x => x.OwningEntityId != null && entityIds.Contains(x.OwningEntityId.Value));
            Conditions(conditions.ToArray());

            foreach (var entity in moddableObjects)
                Graph.Entities.Remove(entity.Id);
        }

        public void Conditions(params Condition[] conditions)
        {
            var mods = conditions.SelectMany(x => x.GetModifiers()).ToArray();
            Mods(mods);

            foreach (var condition in conditions)
                Graph.Conditions.Remove(condition);
        }

        public void Mods(params Modifier[] mods) 
        {
            foreach (var mod in mods)
            {
                var modified = Graph.Mods.Iterate(mod, (modProp) =>
                {
                    var removed = modProp.Remove(mod);
                    if (removed != null)
                        Graph.NotifyOp.Queue(modProp);

                    return removed != null;
                });
            }

            Graph.NotifyOp.Send();
        }
    }
}
