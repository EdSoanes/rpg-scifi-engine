using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class UpdateOp : Operation
    {
        public UpdateOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public void Conditions(params Condition[] conditions)
        {
            var toRemove = new List<Condition>();
            var toExpire = new List<Condition>();

            if (!conditions.Any())
                conditions = ConditionStore.ToArray();

            foreach (var condition in conditions)
            {
                var expiry = condition.Duration.GetExpiry(Graph.Turn);
                if (expiry == ModifierExpiry.Remove || expiry == ModifierExpiry.Expired)
                    toRemove.Add(condition);
                else
                    toExpire.Add(condition);
            }

            Graph.Expire.Conditions(toExpire.ToArray());
            Graph.Remove.Conditions(toRemove.ToArray());
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
                        Graph.Notify.Queue(modProp);
                    }
                }
            }

            Graph.Expire.Mods(toExpire.ToArray());
            Graph.Remove.Mods(toRemove.ToArray());

            Graph.Notify.Send();
        }

        private List<ModProp> GetModProps(IEnumerable<Modifier> mods)
        {
            var modProps = new List<ModProp>();
            
            if (!mods.Any())
                modProps = ModStore.Values.ToList();
            else
            {
                foreach (var mod in mods)
                {
                    ModStore.Iterate(mod, (modProp) =>
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
