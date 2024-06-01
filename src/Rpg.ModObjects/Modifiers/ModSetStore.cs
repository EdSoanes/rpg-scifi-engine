using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects.Modifiers
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
                    Items.Add(modSet.Id, modSet);
                    Graph!.AddMods(modSet.Mods.ToArray());

                    return true;
                }
            }

            return false;
        }

        public void Remove(string modSetId)
        {
            var existing = Get().FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                existing.SetExpired();
                Graph?.RemoveMods(existing.Mods.ToArray());
                Items.Remove(existing.Id);
            }
        }

        public void RemoveByName(string name)
        {
            var existing = Get().FirstOrDefault(x => x.Name == name);
            if (existing != null)
            {
                Graph!.RemoveMods(existing.Mods.ToArray());
                Items.Remove(existing.Id);
            }
        }

        public override void OnUpdating(RpgGraph graph)
        {
            base.OnUpdating(graph);

            var toRemove = new List<ModSet>();
            foreach (var modSet in Get())
            {
                modSet.OnUpdating(graph);
                if (modSet.Behavior.Expiry == ModExpiry.Remove)
                    toRemove.Add(modSet);
            }

            foreach (var remove in toRemove)
                Items.Remove(remove.Id);
        }
    }
}
