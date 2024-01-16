using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Add : Operation
    {
        public Add(Graph graph) 
            : base(graph) { }

        public override void Execute(params ModdableObject[] entities)
        {
            var moddableObjects = new List<ModdableObject>();
            foreach (var entity in entities)
            {
                var modObjs = Descendants(entity);
                moddableObjects.AddRange(modObjs);
            }

            foreach (var moddableObject in moddableObjects)
            {
                moddableObject.OnAdd(Graph);

                if (!Graph.Entities.ContainsKey(moddableObject.Id))
                {
                    Graph.Entities.Add(moddableObject.Id, moddableObject);
                    CreateModProps(moddableObject);
                }
            }

            if (!Restoring)
            {
                var mods = moddableObjects.SelectMany(x => x.OnSetup());
                mods.Reverse();
                Execute(mods.ToArray());
            }
        }

        public override void Execute(params Condition[] conditions)
        {
            foreach (var condition in conditions)
            {
                Graph.Conditions.Add(condition);
                Execute(condition.GetModifiers());
            }
        }

        public override void Execute(params Modifier[] mods) 
        {
            foreach (var mod in mods)
            {
                mod.OnAdd(Graph.Turn);

                var modProp = Graph.Mods.Get(mod.Target.Id, mod.Target.Prop);
                if (modProp != null)
                {
                    modProp.Add(mod);
                    AddPropertyChanged(modProp);
                }
            }

            NotifyPropertyChanged();
        }

        private void CreateModProps(ModdableObject entity)
        {
            foreach (var propInfo in entity.ModdableProperties())
            {
                var modProp = Graph.Mods.Get(entity.Id, propInfo.Name);
                if (modProp == null)
                {
                    modProp = new ModProp(entity.Id, propInfo.Name, propInfo.PropertyType.Name);
                    Graph.Mods.Add(modProp);
                }
            }
        }
    }
}
