using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Moddable
{
    public class ModObjectProp : ModObjectPropRef
    {
        [JsonProperty] public List<Mod> Modifiers { get; private set; } = new List<Mod>();

        public ModObjectProp(Guid entityId, string prop)
            : base(entityId, prop)
        {
        }

        public Mod[] Get(ModGraph graph)
        {
            var activeModifiers = Modifiers
                .Where(x => x.Duration.GetExpiry(graph.Turn) == ModExpiry.Active);

            var res = activeModifiers
                .Where(x => x.ModifierType != ModType.Base && x.ModifierType != ModType.BaseOverride)
                .ToList();

            var baseMods = activeModifiers
                .Where(x => x.ModifierType == ModType.BaseOverride);

            if (!baseMods.Any())
                baseMods = activeModifiers
                    .Where(x => x.ModifierType == ModType.Base);

            res.AddRange(baseMods);
            return res.ToArray();
        }

        //public Mod[] Get(ModGraph graph, ModType modifierType)
        //    => Modifiers
        //        .Where(x => x.Duration.GetExpiry(graph.Turn) == ModExpiry.Active && x.ModifierType == modifierType)
        //        .ToArray();

        public Mod[] Get(ModType modifierType, string modName)
            => Modifiers
                .Where(x => x.ModifierType == modifierType && x.Name == modName)
                .ToArray();

        public bool Contains(Mod mod)
            => Modifiers
                .Any(x => x.Id == mod.Id);

        public void Add(Mod mod)
        {
            if (!Modifiers.Contains(mod))
                Modifiers.Add(mod);
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
            var toRemove = Modifiers.FirstOrDefault(x => x.Id == id);

            if (toRemove != null)
                Modifiers.Remove(toRemove);
            return toRemove;
        }

        public Mod[] Remove(params Mod[] mods)
        {
            var toRemove = Modifiers
                .Where(x => mods.Select(m => m.Id).Contains(x.Id))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var mod in toRemove)
                    Modifiers.Remove(mod);
            }

            return toRemove;
        }

        public Mod[] Clear(Graph graph)
        {
            if (Modifiers.Any())
            {
                var res = Modifiers.ToArray();
                Modifiers.Clear();
                return res;
            }

            return new Mod[0];
        }

        public bool Clean(Graph graph)
        {
            var toRemove = Modifiers
                .Where(x => x.Duration.CanRemove(graph.Turn))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var remove in toRemove)
                    Modifiers.Remove(remove);

                return true;
            }

            return false;
        }

        public bool IsAffectedBy(ModObjectPropRef propRef)
            => Modifiers
                .Any(x => x.Source.PropRef != null && x.Source.PropRef == propRef);
    }
}
