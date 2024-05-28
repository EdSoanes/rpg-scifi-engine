using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Stores
{
    public class ModPropStore : ModBaseStore<string, ModProp>
    {
        public string[] GetPropNames()
            => Items.Keys.ToArray();

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

        public Mod[] GetMods(string prop, ModType modType)
            => this[prop]?.Get(modType) ?? new Mod[0];

        public Mod[] GetMods(string prop, ModType modType, string modName)
            => this[prop]?.Get(modType, modName) ?? new Mod[0];

        public ModProp Create(string prop)
        {
            if (!Contains(prop))
                this[prop] = new ModProp(EntityId, prop);

            return this[prop]!;
        }

        public Dice Calculate(string prop, Func<Mod, bool>? filterFunc = null)
        {
            var mods = GetMods(prop)
                .Where(x => filterFunc?.Invoke(x) ?? true);

            return this[prop]?.Calculate(Graph!, mods) ?? Dice.Zero;
        }

        public Dice CalculateInitialValue(string prop)
        {
            var mods = GetMods(prop)
            .Where(x => x.IsBaseInitMod);

            return this[prop]?.Calculate(Graph!, mods) ?? Dice.Zero;
        }

        public Dice CalculateBaseValue(string prop)
        {
            var mods = GetMods(prop)
                .Where(x => x.IsBaseMod);

            return this[prop]?.Calculate(Graph!, mods) ?? Dice.Zero;
        }

        public void Remove(Mod mod)
            => Remove(new[] { mod });

        public void Remove(IEnumerable<Mod> mods)
        {
            var toRemove = new List<ModProp>();
            foreach (var mod in mods)
            {
                var modProp = Graph?.GetEntity<ModObject>(mod.EntityId)?.GetModProp(mod.Prop);
                if (modProp != null)
                {
                    modProp.Remove(mod);
                    if (!modProp.Mods.Any() && !toRemove.Contains(modProp))
                        toRemove.Add(modProp);
                }
            }

            foreach (var modProp in toRemove)
            {
                var entity = Graph?.GetEntity<ModObject>(modProp.EntityId);
                entity?.RemoveModProp(modProp.Prop);
            }
        }

        public void Add(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.OnAdd(Graph!.Turn);

                var entity = Graph!.GetEntity<ModObject>(mod.EntityId);
                if (entity != null)
                {
                    var modProp = entity.GetModProp(mod.Prop, create: true)!;
                    if (modProp.Contains(mod))
                        modProp.Remove(mod.Id);

                    if (mod.Behavior.Merging == ModMerging.Add)
                        modProp.Add(mod);

                    else if (mod.Behavior.Merging == ModMerging.Replace)
                        modProp.Replace(mod);

                    else if (mod.Behavior.Merging == ModMerging.Combine)
                        modProp.Sum(Graph, entity, mod);
                }
            }
        }

        public List<ModPropRef> GetPropsAffectedBy(ModPropRef propRef)
        {
            var res = new List<ModPropRef>();
            res.Merge(propRef);

            var propsAffectedBy = new List<ModPropRef>();
            foreach (var entity in Graph!.GetEntities())
            {
                var affectedBy = entity.GetModProps()
                    .Where(x => x.IsAffectedBy(propRef))
                    .Select(x => new ModPropRef(entity.Id, x.Prop))
                    .Distinct();

                res.Merge(affectedBy);

                foreach (var propAffectedBy in affectedBy)
                {
                    var parentEntity = Graph!.GetEntity<ModObject>(propAffectedBy.EntityId);
                    var parentAffects = parentEntity!.GetPropsAffectedBy(propAffectedBy.Prop);

                    res.Merge(parentAffects);
                }
            }

            return res;
        }

        public List<ModPropRef> AffectedByProps()
        {
            var res = Items.Keys
                .SelectMany(GetPropsThatAffect)
                .Distinct()
                .ToList();

            return res;
        }

        public List<ModPropRef> GetPropsThatAffect(string prop)
        {
            var res = new List<ModPropRef>();

            var affectedByGroup = GetMods(prop)
                .Where(x => x.Source.EntityId != null && !string.IsNullOrEmpty(x.Source.Prop))
                .Select(x => new ModPropRef(x.Source.EntityId!.Value, x.Source.Prop!))
                .Distinct()
                .GroupBy(x => x.EntityId);

            foreach (var affectedByProp in affectedByGroup)
            {
                var entity = Graph!.GetEntity<ModObject>(affectedByProp.Key);
                if (entity != null)
                {
                    var childProps = affectedByProp
                        .SelectMany(x => entity.GetPropsThatAffect(x.Prop))
                        .Distinct();

                    res.Merge(childProps);
                }

                res.Merge(affectedByProp);
            }

            res.Merge(new ModPropRef(EntityId, prop));
            return res;
        }

        public override void OnTurnChanged(int turn) { }
        public override void OnBeginEncounter() { }

        public override void OnEndEncounter()
        {
            foreach (var modProp in Items.Values)
            {
                var toRemove = new List<Mod>();
                foreach (var mod in modProp.Mods)
                {
                    var expiry = mod.Behavior.GetExpiry(Graph, mod);
                    if (expiry == ModExpiry.Expired && mod.Behavior.CanRemove(Graph, mod))
                        toRemove.Add(mod);
                }

                if (toRemove.Any())
                {
                    Remove(toRemove);
                }
            }
        }
    }
}
