using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Props
{
    public class Prop : PropRef
    {
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();

        public Prop(string entityId, string prop)
            : base(entityId, prop)
        {
        }

        public Mod[] GetActive()
        {
            if (Mods.Any(x => x.IsBaseOverrideMod))
                return Mods
                    .Where(x => x.Expiry == LifecycleExpiry.Active && (x.IsBaseOverrideMod || x.Behavior is Threshold))
                    .ToArray();

            return Mods
                    .Where(x => x.Expiry == LifecycleExpiry.Active)
                    .ToArray();
        }

        public Mod[] Get(Func<Mod, bool> filterFunc)
            => Mods
                .Where(x => filterFunc(x))
                .ToArray();

        public bool Contains(Mod mod)
            => Mods
                .Any(x => x.Id == mod.Id);

        public void Add(Mod mod)
        {
            if (!Contains(mod))
                Mods.Add(mod);
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
                .Where(x => x.Expiry == LifecycleExpiry.Remove)
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
