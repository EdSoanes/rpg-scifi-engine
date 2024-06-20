using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Props
{
    public class PropStore : ModBaseStore<string, Prop>
    {
        public PropStore(string entityId)
            : base(entityId) { }

        public Prop? Get(string prop, bool create = false)
        {
            var modProp = this[prop];

            if (modProp == null && create)
                modProp = Create(prop);

            return modProp;
        }

        public Mod[] GetMods(bool filtered = true)
        {
            return filtered
                ? Items.Values.SelectMany(x => x.Get(Graph!)).ToArray()
                : Items.Values.SelectMany(x => x.Mods).ToArray();
        }

        public Mod[] GetMods(string prop, bool filtered = true)
        {
            var res = filtered
                ? this[prop]?.Get(Graph!)
                : this[prop]?.Mods?.ToArray();

            return res ?? new Mod[0];
        }

        public Mod[] GetMods(string prop, Func<Mod, bool> filterFunc)
            => this[prop]?.Mods.Where(x => filterFunc(x)).ToArray() ?? Array.Empty<Mod>();

        public Prop Create(string prop)
        {
            if (!Contains(prop))
                this[prop] = new Prop(EntityId, prop);

            return this[prop]!;
        }

        public void Add(params Mod[] mods)
        {
            foreach (var mod in mods.Where(x => x.EntityId == EntityId))
            {
                var modProp = Get(mod.Prop, create: true)!;
                modProp.Remove(mod);

                mod.OnAdding(Graph!, modProp, Graph!.Time.Current);

                Graph.OnPropUpdated(modProp);
            }
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? modx = null)
        {
            var res = base.OnUpdateLifecycle(graph, currentTime, modx);

            var toRemove = new List<Mod>();
            foreach (var modProp in Items.Values)
            {
                foreach (var mod in modProp.Mods)
                {
                    var oldExpiry = mod.Expiry;
                    mod.OnUpdating(Graph, modProp, currentTime);
                    var expiry = mod.Expiry;

                    if (expiry == LifecycleExpiry.Remove)
                        toRemove.Add(mod);

                    if (expiry != oldExpiry)
                        Graph.OnPropUpdated(mod);
                }
            }

            Remove(toRemove.ToArray());

            return res;
        }

        public void Remove(params Mod[] mods)
        {
            var toRemove = new List<Prop>();
            foreach (var mod in mods.Where(x => x.EntityId == EntityId))
            {
                var modProp = this[mod.Prop];
                if (modProp != null)
                {
                    mod.OnRemoving(Graph!, modProp, mod);
                    modProp.Remove(mod);

                    if (!modProp.Mods.Any() && !toRemove.Contains(modProp))
                        toRemove.Add(modProp);

                    Graph.OnPropUpdated(modProp);
                }
            }

            foreach (var modProp in toRemove)
                this.Remove(modProp.Prop);
        }
    }
}
