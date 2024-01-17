using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Restore : Operation
    {
        private readonly Add _add;

        public Restore(Graph graph) 
            : base(graph) 
        {
            _add = new Add(graph);
            _add.Restoring = true;
        }

        public void Entities(params ModdableObject[] entities)
        {
            Graph.Entities.Clear();
            _add.Entities(entities);
        }

        public void Conditions(params Condition[] conditions)
        {
            Graph.Conditions.Clear();
            foreach (var condition in conditions)
                Graph.Conditions.Add(condition);
        }

        public void Mods(params Modifier[] mods) 
        {
            foreach (var group in mods.GroupBy(x => x.Target.Id))
            {
                var entity = Graph.Entities.Get(group.Key);
                if (entity != null)
                {
                    foreach (var propInfo in entity.ModdableProperties())
                    {
                        var modProp = Graph.Mods.Get(entity.Id, propInfo.Name);
                        if (modProp == null)
                        {
                            modProp = new ModProp(entity.Id, propInfo.Name, propInfo.PropertyType.Name);
                            Graph.Mods.Add(modProp);

                            var entityMods = group.Where(x => x.Target.Prop == propInfo.Name).ToArray();
                            modProp.Add(entityMods);
                        }

                        Graph.NotifyOp.Queue(modProp);
                    }
                }
            }

            Graph.NotifyOp.Send();
        }
    }
}
