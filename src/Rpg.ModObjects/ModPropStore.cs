using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class ModPropStore : ITemporal
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public Guid EntityId { get; set; }

        [JsonProperty] protected Dictionary<string, ModProp> ModProps { get; set; } = new Dictionary<string, ModProp>();

        public ModProp? this[string prop]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(prop))
                    return null;

                if (ModProps.ContainsKey(prop))
                    return ModProps[prop];

                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value?.Prop) && !ModProps.ContainsKey(prop))
                    ModProps.Add(prop, value);
            }
        }

        private ModPropStore(Dictionary<string, ModProp> modObjProps)
            => ModProps = modObjProps;

        public ModPropStore() { }

        public string[] GetPropNames()
            => ModProps.Keys.ToArray();

        public Mod[] Get(bool filtered = true)
        {
            return filtered
                ? ModProps.Values.SelectMany(x => x.Get(Graph!)).ToArray()
                : ModProps.Values.SelectMany(x => x.Mods).ToArray();
        }

        public Mod[] Get(string prop, bool filtered = true)
        {
            return filtered
                ? ModProps[prop].Get(Graph!)
                : ModProps.Values.SelectMany(x => x.Mods).ToArray();
        }

        public Mod[] Get(string prop, ModType modType, string modName)
            => ModProps[prop].Get(modType, modName);

        public ModProp[] GetModProps()
            => ModProps.Values.ToArray();

        public bool Contains(string prop)
            => ModProps.ContainsKey(prop);

        public ModProp Create(string prop)
        {
            if (!Contains(prop))
                this[prop] = new ModProp(EntityId, prop);

            return this[prop]!;
        }

        public Dice Calculate(string prop, ModType? modifierType = null, string? modifierName = null)
        {
            var mods = !string.IsNullOrEmpty(modifierName) && modifierType != null
                ? Get(prop, modifierType.Value, modifierName)
                : Get(prop);

            return Calculate(mods);
        }

        public Dice CalculateInitialValue(string prop)
        {
            var mods = Get(prop)
            .Where(x => x.IsBaseInitMod);

            return Calculate(mods);
        }

        public Dice CalculateBaseValue(string prop)
        {
            var mods = Get(prop)
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
            var res = ModProps.Keys
                .SelectMany(GetPropsThatAffect)
                .Distinct()
                .ToList();

            return res;
        }

        public List<ModPropRef> GetPropsThatAffect(string prop)
        {
            var res = new List<ModPropRef>();

            var affectedByGroup = Get(prop)
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

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnTurnChanged(int turn) { }
        public void OnBeginEncounter() { }

        public void OnEndEncounter()
        {
            var turn = Graph!.Turn;

            foreach (var modProp in ModProps.Values)
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
