using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public class ModFilters
    {
        public static bool IsActive(Mod mod)
            => mod.Expiry == LifecycleExpiry.Active;

        public static bool IsExpired(Mod mod)
            => mod.Expiry == LifecycleExpiry.Expired || mod.Expiry == LifecycleExpiry.Destroyed;

        public static bool IsInitial(Mod mod)
            => IsActive(mod) && mod is Initial || mod is Mods.Threshold;

        public static bool IsBase(Mod mod)
            => IsActive(mod) && mod is Initial || mod is Base || mod is Override || mod is Mods.Threshold;

        public static bool IsOriginalBase(Mod mod)
            => IsActive(mod) && mod is Initial || mod is Base || mod is Mods.Threshold;

        public static bool IsOverride(Mod mod)
            => IsActive(mod) && mod is Override;

        public static bool IsThreshold(Mod mod)
            => IsActive(mod) && mod is Override;

        public IEnumerable<Mod> Active(RpgGraph graph, PropRef propRef)
            => Active(graph, propRef.EntityId, propRef.Prop);

        public IEnumerable<Mod> Active(RpgGraph graph, string entityId, string prop)
        {
            var rpgObj = graph.GetObject(entityId);
            var mods = rpgObj?.GetMods(prop) ?? [];
            return Active(mods);
        }

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
            => Active(mods).Where(x => !(x is Mods.Threshold));

        public static Mod? ActiveThreshold(IEnumerable<Mod> mods)
            => Active(mods).FirstOrDefault(x => x is Mods.Threshold);

        public static IEnumerable<Mod> ActiveByOwner(IEnumerable<Mod> mods, string? ownerId)
            => ownerId == null
            ? []
            : Active(mods).Where(x => x.OwnerId == ownerId);

        public static IEnumerable<Mod> SyncedToOwner(IEnumerable<Mod> mods, string? ownerId)
            => ownerId == null
            ? []
            : Active(mods).Where(x => x.OwnerId == ownerId);

        public static IEnumerable<Mod> ActiveMatching<T>(IEnumerable<Mod> mods, Mod mod)
            where T : BaseBehavior
            => mods.Where(x => x.Behavior is T && x.GetType() == mod.GetType() && x.Name == mod.Name);
    }
}
