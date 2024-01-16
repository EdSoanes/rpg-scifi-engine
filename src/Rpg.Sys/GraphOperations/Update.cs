using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Update : Operation
    {
        private readonly Expire _expire;
        private readonly Remove _remove;

        public Update(Graph graph, Expire expire, Remove remove) 
            : base(graph) 
        {
            _expire = expire;
            _remove = remove;
        }

        public override void Execute(params Condition[] conditions)
        {
            var toRemove = new List<Condition>();
            var toExpire = new List<Condition>();

            foreach (var condition in conditions)
            {
                var expiry = condition.Duration.GetExpiry(Graph.Turn);
                if (expiry == ModifierExpiry.Remove || expiry == ModifierExpiry.Expired)
                    toRemove.Add(condition);
                else
                    toExpire.Add(condition);
            }

            _expire.Execute(toExpire.ToArray());
            _remove.Execute(toRemove.ToArray());
        }

        public override void Execute(params Modifier[] mods) 
        {
            var toRemove = new List<Modifier>();
            var toExpire = new List<Modifier>();

            var modProps = GetModProps(mods);
            foreach (var modProp in modProps)
            {
                foreach (var mod in modProp.AllModifiers)
                {
                    mod.OnUpdate(Graph.Turn);

                    var expiry = mod.Duration.GetExpiry(Graph.Turn);
                    if (expiry == ModifierExpiry.Remove)
                    {
                        toRemove.Add(mod);
                    }
                    else if (expiry == ModifierExpiry.Expired)
                    {
                        toExpire.Add(mod);
                    }
                    else if (expiry != mod.Duration.GetExpiry(Graph.Turn - 1))
                    {
                        AddPropertyChanged(modProp);
                    }
                }
            }

            _expire.Execute(toExpire.ToArray());
            _remove.Execute(toRemove.ToArray());

            NotifyPropertyChanged();
        }

        private List<ModProp> GetModProps(IEnumerable<Modifier> mods)
        {
            var modProps = new List<ModProp>();
            
            if (!mods.Any())
                modProps = Graph.Mods.Values.ToList();
            else
            {
                foreach (var mod in mods)
                {
                    Graph.Mods.Iterate(mod, (modProp) =>
                    {
                        if (!modProps.Any(x => x.EntityId == modProp.EntityId && x.Prop == modProp.Prop))
                            modProps.Add(modProp);
                        return true;
                    });
                }
            }

            return modProps;
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
