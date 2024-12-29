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
            => IsActive(mod) && (mod.ModType == ModType.Initial || mod.ModType == ModType.Threshold);

        public static bool IsBase(Mod mod)
            => IsOriginalBase(mod)  || mod.ModType == ModType.Override;

        public static bool IsOriginalBase(Mod mod)
            => IsActive(mod) && (mod.ModType == ModType.Initial || mod.ModType == ModType.Base || mod.ModType == ModType.Threshold);

        public static bool IsOverride(Mod mod)
            => IsActive(mod) && mod.ModType == ModType.Override;

        public static bool IsThreshold(Mod mod)
            => IsActive(mod) && mod.ModType == ModType.Threshold;

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

            return mods
                .Where(IsActive)
                .ToArray();
        }

        public static IEnumerable<Mod> ActiveNoThreshold(IEnumerable<Mod> mods)
            => Active(mods).Where(x => x.ModType != ModType.Threshold);

        public static Mod? ActiveThreshold(IEnumerable<Mod> mods)
            => Active(mods).FirstOrDefault(x => x.ModType == ModType.Threshold);
    }
}
