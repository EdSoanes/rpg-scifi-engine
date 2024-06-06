using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Props
{
    public class Prop : PropRef
    {
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();

        public Prop(string entityId, string prop)
            : base(entityId, prop)
        {
        }

        public Mod[] Get(RpgGraph graph)
        {
            var activeModifiers = Mods
                .Where(x => x.Expiry == ModExpiry.Active);

            var res = activeModifiers
                .Where(x => !x.IsBaseMod)
                .ToList();

            var baseMods = activeModifiers
                .Where(x => x.IsBaseOverrideMod);

            if (!baseMods.Any())
                baseMods = activeModifiers
                    .Where(x => x.IsBaseMod);

            res.AddRange(baseMods);
            return res.ToArray();
        }

        public Mod[] Get(ModType modType)
            => Mods
                .Where(x => x.Behavior.Type == modType)
                .ToArray();

        public Mod[] Get(ModType modType, string modName)
            => Mods
                .Where(x => x.Behavior.Type == modType && x.Name == modName)
                .ToArray();

        public bool Contains(Mod mod)
            => Mods
                .Any(x => x.Id == mod.Id);

        public void Add(Mod mod)
        {
            if (!Contains(mod))
                Mods.Add(mod);
        }

        internal void Combine(RpgGraph graph, Mod mod)
        {
            var entity = graph.GetEntity(mod.EntityId);
            var oldValue = graph.CalculatePropValue(entity, mod.Prop, x => x.Behavior.Type == mod.Behavior.Type && x.Name == mod.Name);
            var newValue = (oldValue ?? Dice.Zero) + graph?.CalculateModValue(mod) ?? Dice.Zero;

            mod.SetSource(newValue);
            var oldMods = Get(mod.Behavior.Type, mod.Name);
            foreach (var oldMod in oldMods)
                Remove(oldMod);

            if (mod.SourceValue != null && mod.SourceValue != Dice.Zero)
                Add(mod);
        }

        internal void Replace(Mod mod)
        {
            var oldMods = Get(mod.Behavior.Type, mod.Name);
            foreach (var oldMod in oldMods)
                Remove(oldMod);

            //Don't add if the source is a Value without a ValueFunction and the Value = null
            if (mod.SourcePropRef != null || mod.SourceValue != null || mod.SourceValueFunc.IsCalc)
                Add(mod);
        }

        public Mod? Remove(string id)
        {
            var toRemove = Mods.FirstOrDefault(x => x.Id == id);
            if (toRemove != null)
                Mods.Remove(toRemove);
            return toRemove;
        }

        public Mod? Remove(Mod mod)
            => Remove(mod.Id);

        public Mod[] Clear()
        {
            if (Mods.Any())
            {
                var res = Mods.ToArray();
                Mods.Clear();
                return res;
            }

            return new Mod[0];
        }

        public bool Clean(RpgGraph graph)
        {
            var toRemove = Mods
                .Where(x => x.Expiry == ModExpiry.Remove)
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var remove in toRemove)
                    Mods.Remove(remove);

                return true;
            }

            return false;
        }

        public bool IsAffectedBy(PropRef propRef)
            => Mods
                .Any(x => x.SourcePropRef != null && x.SourcePropRef.EntityId == propRef.EntityId && x.SourcePropRef.Prop == propRef.Prop);
    }
}
