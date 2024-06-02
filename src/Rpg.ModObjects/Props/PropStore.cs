using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Stores;

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
                mod.Behavior.OnAdding(Graph, mod);

                var modProp = Get(mod.Prop, create: true)!;
                modProp.Remove(mod);

                if (mod.Behavior.Merging == ModMerging.Add)
                    modProp.Add(mod);

                else if (mod.Behavior.Merging == ModMerging.Replace)
                    modProp.Replace(mod);

                else if (mod.Behavior.Merging == ModMerging.Combine)
                    modProp.Combine(Graph, mod);

                Graph.OnPropUpdated(modProp);
            }
        }

        public void Remove(params Mod[] mods)
        {
            var toRemove = new List<Prop>();
            foreach (var mod in mods.Where(x => x.EntityId == EntityId))
            {
                mod.OnRemoving(Graph!, mod);

                var modProp = this[mod.Prop];
                if (modProp != null)
                {
                    modProp.Remove(mod);
                    if (mod.Behavior.Scope != ModScope.Standard)
                    if (!modProp.Mods.Any() && !toRemove.Contains(modProp))
                        toRemove.Add(modProp);

                    Graph.OnPropUpdated(modProp);
                }
            }

            foreach (var modProp in toRemove)
                this.Remove(modProp.Prop);
        }

        public override void OnUpdating(RpgGraph graph, Time.Time time)
        {
            base.OnUpdating(graph, time);
            foreach (var modProp in Items.Values)
            {
                var toRemove = new List<Mod>();
                foreach (var mod in modProp.Mods)
                {
                    var oldExpiry = mod.Behavior.Expiry;
                    mod.OnUpdating(Graph, time);
                    var expiry = mod.Behavior.Expiry;

                    if (expiry != oldExpiry)
                        Graph.OnPropUpdated(mod);

                    if (expiry == ModExpiry.Remove)
                        toRemove.Add(mod);
                }

                Remove(toRemove.ToArray());
            }
        }
    }
}
