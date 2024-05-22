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
            return filtered
                ? Items[prop].Get(Graph!)
                : Items.Values.SelectMany(x => x.Mods).ToArray();
        }

        public Mod[] GetMods(string prop, ModType modType, string modName)
            => Items[prop].Get(modType, modName);

        public ModProp Create(string prop)
        {
            if (!Contains(prop))
                this[prop] = new ModProp(EntityId, prop);

            return this[prop]!;
        }

        public Dice Calculate(string prop, ModType? modifierType = null, string? modifierName = null)
        {
            var mods = !string.IsNullOrEmpty(modifierName) && modifierType != null
                ? GetMods(prop, modifierType.Value, modifierName)
                : GetMods(prop);

            return Calculate(mods);
        }

        public Dice CalculateInitialValue(string prop)
        {
            var mods = GetMods(prop)
            .Where(x => x.IsBaseInitMod);

            return Calculate(mods);
        }

        public Dice CalculateBaseValue(string prop)
        {
            var mods = GetMods(prop)
                .Where(x => x.IsBaseMod);

            return Calculate(mods);
        }

        private Dice Calculate(IEnumerable<Mod> mods)
        {
            Dice dice = "0";
            foreach (var mod in mods)
            {
                Dice value = mod.Source.CalculatePropValue(Graph!);
                dice += value;
            }

            return dice;
        }

        public void Remove(Mod mod)
            => Remove(new[] { mod });

        public void Remove(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                var entity = Graph!.GetEntity<ModObject>(mod.EntityId);
                if (entity != null)
                    entity.GetModProp(mod.Prop)!.Remove(mod);
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

                    if (mod.ModifierAction == ModAction.Accumulate)
                        modProp.Add(mod);

                    else if (mod.ModifierAction == ModAction.Replace)
                        modProp.Replace(mod);

                    else if (mod.ModifierAction == ModAction.Sum)
                    {
                        var oldValue = entity.CalculatePropValue(mod.Prop, mod.ModifierType, mod.Name);
                        var newValue = (oldValue ?? Dice.Zero) + mod.Source.CalculatePropValue(Graph!);

                        mod.SetSource(newValue);
                        modProp.Replace(mod);
                    }
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
            var turn = Graph!.Turn;

            foreach (var modProp in Items.Values)
            {
                var toRemove = new List<Mod>();
                foreach (var mod in modProp.Mods)
                {
                    mod.OnUpdate(turn);

                    var expiry = mod.Duration.GetExpiry(turn);
                    if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(Graph!.Turn))
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
