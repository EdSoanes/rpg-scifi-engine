using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public class ModSetStore : ModBaseStore<string, ModSet>
    {
        public ModSetStore(string entityId)
            : base(entityId) { }

        public ModSet? Get(string name)
            => Get().FirstOrDefault(x => x.Name == name);

        public bool Add(ModSet modSet)
        {
            if (!Contains(modSet))
            {
                if (string.IsNullOrEmpty(modSet.Name) || !Get().Any(x => x.Name == modSet.Name))
                {
                    modSet.OnBeginningOfTime(Graph!, Graph!.GetEntity(EntityId));
                    Items.Add(modSet.Id, modSet);
                    Graph!.AddMods(modSet.Mods.ToArray());

                    return true;
                }
            }

            return false;
        }

        public override bool Remove(string modSetId)
        {
            var existing = Get().FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                existing.Lifecycle.SetExpired(Graph!.Time.Current);
                Graph?.RemoveMods(existing.Mods.ToArray());
                Items.Remove(existing.Id);

                return true;
            }

            return false;
        }

        public void RemoveByName(string name)
        {
            var existing = Get().FirstOrDefault(x => x.Name == name);
            if (existing != null)
                Remove(existing.Id);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime, mod);
            var toRemove = new List<ModSet>();
            foreach (var modSet in Get())
            {
                modSet.OnUpdateLifecycle(graph, currentTime);
                if (modSet.Lifecycle.Expiry == LifecycleExpiry.Remove)
                    toRemove.Add(modSet);
            }

            foreach (var remove in toRemove)
                Remove(remove.Id);

            return expiry;
        }
    }
}
