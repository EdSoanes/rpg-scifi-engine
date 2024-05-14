using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class ModProp : ModPropRef
    {
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();

        public ModProp(Guid entityId, string prop)
            : base(entityId, prop)
        {
        }

        public Mod[] Get(ModGraph graph)
        {
            var activeModifiers = Mods
                .Where(x => x.Duration.GetExpiry(graph.Turn) == ModExpiry.Active);

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

        public Mod[] Get(ModType modifierType, string modName)
            => Mods
                .Where(x => x.ModifierType == modifierType && x.Name == modName)
                .ToArray();

        public bool Contains(Mod mod)
            => Mods
                .Any(x => x.Id == mod.Id);

        public void Add(Mod mod)
        {
            if (!Mods.Contains(mod))
                Mods.Add(mod);
        }

        public void Replace(Mod mod)
        {
            var oldMods = Get(mod.ModifierType, mod.Name);
            Remove(oldMods);

            //Don't add if the source is a Value without a ValueFunction and the Value = 0
            if (mod.Source.Value == null || mod.Source.Value != Dice.Zero || mod.Source.ValueFunc.IsCalc)
                Add(mod);
        }

        public Mod? Remove(Guid id)
        {
            var toRemove = Mods.FirstOrDefault(x => x.Id == id);

            if (toRemove != null)
                Mods.Remove(toRemove);
            return toRemove;
        }

        public Mod[] Remove(params Mod[] mods)
        {
            var toRemove = Mods
                .Where(x => mods.Select(m => m.Id).Contains(x.Id))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var mod in toRemove)
                    Mods.Remove(mod);
            }

            return toRemove;
        }

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

        public bool Clean(ModGraph graph)
        {
            var toRemove = Mods
                .Where(x => x.Duration.CanRemove(graph.Turn))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var remove in toRemove)
                    Mods.Remove(remove);

                return true;
            }

            return false;
        }

        public bool IsAffectedBy(ModPropRef propRef)
            => Mods
                .Any(x => x.Source.PropRef != null && x.Source.PropRef == propRef);
    }
}
