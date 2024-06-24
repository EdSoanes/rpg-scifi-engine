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
            if (!Contains(modSet))
            {
                if (string.IsNullOrEmpty(modSet.Name) || !Get().Any(x => x.Name == modSet.Name))
                {
                    var entity = Graph!.GetEntity(EntityId);
                    modSet.OnBeforeTime(Graph, entity);
                    modSet.OnBeginningOfTime(Graph, entity);

                    Items.Add(modSet.Id, modSet);

                    Graph!.AddMods(modSet.Mods.ToArray());
                    modSet.OnStartLifecycle(Graph!, Graph.Time.Current);

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

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            foreach (var modSet in Get())
                modSet.OnBeforeTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var expiry = base.OnStartLifecycle(graph, currentTime, mod);
            
            foreach (var modSet in Get())
                modSet.OnStartLifecycle(graph, currentTime, mod);

            return expiry;
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
