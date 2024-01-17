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

        public void Conditions(params Condition[] conditions)
        {
            var toRemove = new List<Condition>();
            var toExpire = new List<Condition>();

            if (!conditions.Any())
                conditions = Graph.Conditions.ToArray();

            foreach (var condition in conditions)
            {
                var expiry = condition.Duration.GetExpiry(Graph.Turn);
                if (expiry == ModifierExpiry.Remove || expiry == ModifierExpiry.Expired)
                    toRemove.Add(condition);
                else
                    toExpire.Add(condition);
            }

            _expire.Conditions(toExpire.ToArray());
            _remove.Conditions(toRemove.ToArray());
        }

        public void Mods(params Modifier[] mods) 
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
                        Graph.NotifyOp.Queue(modProp);
                    }
                }
            }

            _expire.Mods(toExpire.ToArray());
            _remove.Mods(toRemove.ToArray());

            Graph.NotifyOp.Send();
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
    }
}
