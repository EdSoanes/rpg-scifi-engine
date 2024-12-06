using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Mods
{
    internal class ModCalculator
    {
        internal static Dice? Value(RpgGraph graph, Mod mod)
        {
            if (graph == null)
                return null;

            Dice? value = mod.SourceValue ?? graph.GetObject(mod.Source!.EntityId)?.Value(mod.Source.Prop);

            if (value != null && mod.SourceValueFunc != null)
            {
                var args = new Dictionary<string, object?>();
                args.Add(mod.SourceValueFunc.Args.First().Name, value);

                var entity = graph.GetObject(mod.SourceValueFunc.EntityId);
                value = entity != null
                    ? mod.SourceValueFunc.Execute(entity, args)
                    : mod.SourceValueFunc.Execute(args);
            }

            return value;
        }

        internal static Dice? InitialValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsInitial);
            return Value(graph, baseMods);
        }

        internal static Dice? BaseValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsBase);
            return Value(graph, baseMods);
        }

        internal static Dice? OriginalBaseValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsOriginalBase);
            return Value(graph, baseMods);
        }

        internal static Dice? Value(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var selectedMods = ModFilters.ActiveNoThreshold(mods);

            if (!selectedMods.Any())
                return null;

            Dice? dice = null;
            foreach (var mod in selectedMods)
            {
                var val = Value(graph, mod);
                if (val != null)
                    dice = dice != null ? dice.Value + val.Value : val;
            }

            if (dice == null)
                return null;

            var threshold = ModFilters.ActiveThreshold(mods);
            return ApplyThreshold(threshold, dice!.Value);
        }

        private static Dice ApplyThreshold(Mod? mod, Dice dice)
        {
            var threshold = mod?.Behavior as Threshold;
            if (threshold != null && dice.IsConstant)
            {
                if (dice.Roll() < threshold.Min)
                    dice = threshold.Min;
                else if (dice.Roll() > threshold.Max)
                    dice = threshold.Max;
            }

            return dice;
        }
    }
}
