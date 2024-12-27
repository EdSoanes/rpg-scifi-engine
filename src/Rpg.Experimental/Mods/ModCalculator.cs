using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class ModCalculator
    {
        public static Dice? Value(RpgGraph graph, Mod mod)
        {
            if (graph == null)
                return null;

            var value = ModValue(graph, mod);

            //if (value != null && mod.SourceValueFunc != null)
            //{
            //    var args = new Dictionary<string, object?>();
            //    args.Add(mod.SourceValueFunc.Args.First().Name, value);

            //    var entity = graph.GetObject(mod.SourceValueFunc.EntityId);
            //    value = entity != null
            //        ? mod.SourceValueFunc.Execute(entity, args)
            //        : mod.SourceValueFunc.Execute(args);
            //}

            return value;
        }

        public static Dice? InitialValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsInitial);
            return Value(graph, baseMods);
        }

        public static Dice? BaseValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsBase);
            return Value(graph, baseMods);
        }

        public static Dice? OriginalBaseValue(RpgGraph graph, IEnumerable<Mod> mods)
        {
            var baseMods = mods.Where(ModFilters.IsOriginalBase);
            return Value(graph, baseMods);
        }

        public static Dice? Value(RpgGraph graph, IEnumerable<Mod>? mods)
        {
            if (mods == null) return null;

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

            if (selectedMods.Any(ModFilters.IsOverride))
                return dice;

            var threshold = ModFilters.ActiveThreshold(mods);
            return ApplyThreshold(threshold, dice!.Value);
        }

        public static Dice ApplyThreshold(Mod? mod, Dice dice)
        {
            //var isThreshold = mod != null && mod.ModType == ModType.Threshold;
            //if (isThreshold && dice.IsConstant)
            //{
            //    if (dice.Roll() < isThreshold.Min)
            //        dice = isThreshold.Min;
            //    else if (dice.Roll() > isThreshold.Max)
            //        dice = isThreshold.Max;
            //}

            return dice;
        }

        public static Dice? ModValue(RpgGraph graph, Mod mod)
        {
            var dice = ValueToDice(mod.Source.Value);
            if (dice == null)
            {
                var obj = graph.GetObject(mod.Source.PropertyRef!.ObjectId);
                dice = ValueToDice(obj?.PropertyRefValue(mod.Source.PropertyRef!.Prop)?.Value);
            }

            return dice;
        }

        private static Dice? ValueToDice(object? value)
        {
            Dice? dice = null;
            if (value != null)
            {
                if (value is int)
                    dice = new Dice((int)value);
                else if (value is Dice)
                    dice = (Dice)value;
            }

            return dice;
        }
    }
}
