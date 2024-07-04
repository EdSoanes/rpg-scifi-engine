using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    internal class ModSetStore : RpgBaseStore<string, ModSet>
    {
        public ModSetStore(string entityId)
            : base(entityId) { }

        public ModSet? Get(string name)
            => Get().FirstOrDefault(x => x.Name == name);

        public bool Add(ModSet modSet)
        {
            if (!string.IsNullOrEmpty(modSet.OwnerId) && modSet.OwnerId != EntityId)
                throw new InvalidOperationException("ModSet.OwnerId is set but does not match the ModSetStore.EntityId");

            if (!Contains(modSet.Id))
            {
                var entity = Graph!.GetEntity(EntityId);
                modSet.OnAdding(entity);

                Items.Add(modSet.Id, modSet);

                Graph!.AddMods(modSet.Mods.ToArray());
                modSet.Lifecycle.OnStartLifecycle(Graph!, Graph.Time.Current);

                return true;
            }

            return false;
        }

        public override bool Remove(string modSetId)
        {
            var existing = Get().FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                existing.Lifecycle.SetExpired(Graph!.Time.Current);
                Graph?.RemoveMods(existing.Mods.Where(x => x.Lifecycle is SyncedLifecycle).ToArray());
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

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var expiry = base.OnStartLifecycle(graph, currentTime);

            foreach (var modSet in Get())
            {
                if (!modSet.Mods.Any())
                    modSet.Mods.AddRange(graph.GetActiveMods(x => x.OwnerId == modSet.Id));

                modSet.Lifecycle.OnStartLifecycle(graph, currentTime);
            }

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var expiry = base.OnUpdateLifecycle(graph, currentTime);
            var toRemove = new List<ModSet>();
            foreach (var modSet in Get())
            {
                modSet.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                if (modSet.Lifecycle.Expiry == LifecycleExpiry.Remove)
                    toRemove.Add(modSet);
            }

            foreach (var remove in toRemove)
                Remove(remove.Id);

            return expiry;
        }
    }
}
