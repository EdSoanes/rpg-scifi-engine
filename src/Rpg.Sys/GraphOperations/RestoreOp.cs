using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class RestoreOp : Operation
    {
        private readonly AddOp _add;

        public RestoreOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) 
        {
            _add = new AddOp(graph, mods, entityStore, conditionStore);
            _add.Restoring = true;
        }

        public void Entities(params ModdableObject[] entities)
        {
            EntityStore.Clear();
            _add.Entities(entities);
        }

        public void Conditions(params Condition[] conditions)
        {
            ConditionStore.Clear();
            foreach (var condition in conditions)
                ConditionStore.Add(condition);
        }

        public void Mods(params Modifier[] mods) 
        {
            foreach (var group in mods.GroupBy(x => x.Target.Id))
            {
                var entity = EntityStore.Get(group.Key);
                if (entity != null)
                {
                    foreach (var propInfo in entity.ModdableProperties())
                    {
                        var modProp = ModStore.Get(entity.Id, propInfo.Name);
                        if (modProp == null)
                        {
                            modProp = new ModProp(Graph, entity.Id, propInfo.Name, propInfo.PropertyType.Name);
                            ModStore.Add(modProp);
                        }
                        else
                            modProp.Clear();

                        var entityMods = group.Where(x => x.Target.Prop == propInfo.Name).ToArray();
                        modProp.Add(entityMods);

                        Graph.Notify.Queue(modProp);
                    }
                }
            }

            Graph.Notify.Send();
        }
    }
}
