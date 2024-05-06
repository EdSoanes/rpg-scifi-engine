using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Moddable
{
    public class ModObjectProp : ModObjectPropRef
    {
        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore] public Modifier[] AllModifiers { get => Modifiers.ToArray(); }

        public ModObjectProp(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public Modifier[] BaseModifiers()
            => Modifiers
                .Where(x => x.Duration.GetExpiry(ModGraph.Current.Turn) == ModifierExpiry.Active && x.ModifierType == ModifierType.Base)
                .ToArray();

        public Modifier[] FilteredModifiers()
        {
            var activeModifiers = Modifiers.Where(x => x.Duration.GetExpiry(ModGraph.Current.Turn) == ModifierExpiry.Active);

            var res = activeModifiers
                .Where(x => x.ModifierType != ModifierType.Base && x.ModifierType != ModifierType.BaseOverride)
                .ToList();

            var baseMods = activeModifiers
                .Where(x => x.ModifierType == ModifierType.BaseOverride)
                .ToList();

            if (!baseMods.Any())
                baseMods = activeModifiers
                    .Where(x => x.ModifierType == ModifierType.Base)
                    .ToList();

            res.AddRange(baseMods);
            return res.ToArray();
        }

        public Modifier[] MatchingModifiers(string name, ModifierType modifierType)
            => Modifiers
                .Where(x => x.Name == name && x.ModifierType == modifierType)
                .ToArray();

        public bool AffectedBy(Guid id, string prop)
            => Modifiers
                .Any(x => x.Source != null && x.Source.EntityId == id && x.Source.Prop == prop);

        public bool Contains(Modifier mod)
            => Modifiers
                .Any(x => x.Id == mod.Id);

        public Dice Evaluate(string? modifierName = null, ModifierType? modifierType = null)
        {
            var mods = !string.IsNullOrEmpty(modifierName) && modifierType != null
                ? MatchingModifiers(modifierName, modifierType.Value)
                : FilteredModifiers();

            var newValue = ModGraph.Current.Evaluate.Mod(mods);
            return newValue;
        }

        public bool Add(ModObject entity, params Modifier[] mods)
        {
            var res = false;
            foreach (var mod in mods.Where(x => x.Target.EntityId == EntityId))
            {
                var existing = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
                if (existing != null)
                {
                    Modifiers.Remove(existing);
                    res |= true;
                }

                if (mod.ModifierAction == ModifierAction.Accumulate)
                {
                    Modifiers.Add(mod);
                    res |= true;
                }

                else if (mod.ModifierAction == ModifierAction.Sum)
                {
                    res |= Sum(entity, mod);
                }

                else if (mod.ModifierAction == ModifierAction.Replace)
                {
                    res |= Replace(mod);
                }
            }

            return res;
        }

        private bool Sum(ModObject entity, Modifier mod)
        {
            var oldValue = entity.PropStore.Evaluate(mod.Target.Prop, mod.Name, mod.ModifierType);
            var oldMods = MatchingModifiers(mod.Name, mod.ModifierType);

            Modifiers = Modifiers.Except(oldMods).ToList();
            Modifiers.Add(mod);

            var newValue = entity.PropStore.Evaluate(mod.Target.Prop, mod.Name, mod.ModifierType) + oldValue;
            if (newValue == null || newValue == Dice.Zero && mod.Duration.EndTurn == RemoveTurn.WhenZero)
                Modifiers.Remove(mod);
            else
                mod.SetDice(newValue.Value);

            return true;
        }

        private bool Replace(Modifier mod)
        {
            var matchingMods = Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();

            Modifiers = Modifiers.Except(matchingMods).ToList();
            Modifiers.Add(mod);

            return true;
        }

        public Modifier[] UpdateOnTurn(Graph graph, int newTurn)
        {
            var updated = new List<Modifier>();
            foreach (var mod in Modifiers)
            {
                mod.OnUpdate(newTurn);

                var expiry = mod.Duration.GetExpiry(graph.Turn);
                if (expiry == ModifierExpiry.Remove)
                    updated.Add(mod);
                else
                {
                    var oldExpiry = mod.Duration.GetExpiry(graph.Turn - 1);
                    if (expiry != oldExpiry)
                        updated.Add(mod);
                }
            }

            foreach (var mod in updated.Where(x => x.Duration.GetExpiry(graph.Turn) == ModifierExpiry.Remove))
                Modifiers.Remove(mod);

            return updated.ToArray();
        }

        public Modifier[] Remove(params Modifier[] mods)
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

        public Modifier[] Clear(Graph graph)
        {
            if (Modifiers.Any())
            {
                var res = Modifiers.ToArray();
                Modifiers.Clear();
                return res;
            }

            return new Modifier[0];
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

        public IEnumerable<ModObjectPropRef> AffectedBy()
        {
            var res = new List<ModObjectPropRef>();

            foreach (var mod in FilteredModifiers().Where(x => x.Source != null))
                if (!res.Any(x => x.EntityId == mod.Source!.EntityId && x.Prop == mod.Source.Prop))
                    res.Add(new ModObjectPropRef(mod.Source!.EntityId, mod.Source.Prop));

            return res;
        }
    }
}
