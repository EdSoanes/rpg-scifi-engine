using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class AddOp : Operation
    {
        public AddOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore) 
            : base(graph, mods, entityStore, conditionStore) { }

        public void Entities(params ModdableObject[] entities)
        {
            var moddableObjects = new List<ModdableObject>();
            foreach (var entity in entities)
            {
                var modObjs = entity.Descendants();
                moddableObjects.AddRange(modObjs);
            }

            foreach (var moddableObject in moddableObjects)
            {
                moddableObject.OnAdd(Graph);
                var existing = Graph.Get.Entity<ModdableObject>(moddableObject.Id);
                if (existing == null)
                {
                    EntityStore.Add(moddableObject.Id, moddableObject);
                    CreateModProps(moddableObject);
                }
            }

            if (!Restoring)
            {
                var mods = moddableObjects
                    .SelectMany(x => x.OnSetup())
                    .ToList();

                mods.Reverse();

                Mods(mods.ToArray());
            }
        }

        public void Conditions(params Condition[] conditions)
        {
            foreach (var condition in conditions)
            {
                ConditionStore.Add(condition);
                Mods(condition.GetModifiers());
            }
        }

        public void Mods(params Modifier[] mods) 
        {
            foreach (var mod in mods)
            {
                mod.OnAdd(Graph.Turn);

                var modProp = ModStore.Get(mod.Target.Id, mod.Target.Prop);
                if (modProp != null)
                {
                    modProp.Add(mod);
                    Graph.Notify.Queue(modProp);
                }
            }

            Graph.Notify.Send();
        }

        private void CreateModProps(ModdableObject entity)
        {
            foreach (var propInfo in entity.ModdableProperties())
            {
                var modProp = ModStore.Get(entity.Id, propInfo.Name);
                if (modProp == null)
                {
                    modProp = new ModProp(Graph, entity.Id, propInfo.Name, propInfo.PropertyType.Name);
                    ModStore.Add(modProp);
                }
            }
        }
    }
}
