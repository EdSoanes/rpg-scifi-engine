using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods
{
    public class ModFilters
    {
        public static bool IsActive(Mod mod)
            => mod.Expiry == LifecycleExpiry.Active;

        public static bool IsExpired(Mod mod)
            => mod.Expiry == LifecycleExpiry.Expired || mod.Expiry == LifecycleExpiry.Destroyed;

        public static bool IsInitial(Mod mod)
            => IsActive(mod) && (mod.Type == ModType.Initial || mod.Type == ModType.Threshold);

        public static bool IsBase(Mod mod)
            => IsOriginalBase(mod)  || mod.Type == ModType.Override;

        public static bool IsOriginalBase(Mod mod)
            => IsActive(mod) && (mod.Type == ModType.Initial || mod.Type == ModType.Base || mod.Type == ModType.Threshold);

        public static bool IsOverride(Mod mod)
            => IsActive(mod) && mod.Type == ModType.Override;

        public static bool IsThreshold(Mod mod)
            => IsActive(mod) && mod.Type == ModType.Threshold;

        //public IEnumerable<Mod> Active(RpgGraph graph, RpgPropertyRef propRef)
        //    => Active(graph, propRef.ObjectId, propRef.Prop);

        //public IEnumerable<Mod> Active(RpgGraph graph, string objectId, string prop)
        //{
        //    var rpgObj = graph.GetObject(objectId);
        //    var mods = rpgObj?.GetMods(prop) ?? [];
        //    return Active(mods);
        //}

        public static IEnumerable<Mod> Active(IEnumerable<Mod> mods)
        {
            if (mods.Any(IsOverride))
                return mods
                    .Where(x => IsOverride(x) || IsThreshold(x) || !IsBase(x))
                    .ToArray();

            return FilterReplacements(mods
                .Where(IsActive))
                .ToArray();
        }

        public static IEnumerable<Mod> ActiveNoThreshold(IEnumerable<Mod> mods)
            => Active(mods).Where(x => x.Type != ModType.Threshold);

        public static Mod? ActiveThreshold(IEnumerable<Mod> mods)
            => Active(mods).FirstOrDefault(x => x.Type == ModType.Threshold);

        public static IEnumerable<Mod> FilterReplacements(IEnumerable<Mod> mods)
        {
            var res = mods.Where(x => !(x is Replace)).ToList();

            var groups = mods.Where(x => x is Replace).GroupBy(x => x.Type);
            foreach (var group in groups)
            {
                var prio = group.OrderBy(x => (x as Replace)?.Version ?? -1).LastOrDefault();
                if (prio != null)
                    res.Add(prio);
            }

            return res;
        }
    }
}
