using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSetStore : ModBaseStore<Guid, ModSet>
    {
        public bool Add(ModSet modSet)
        {
            if (!Contains(modSet))
            {
                if (string.IsNullOrEmpty(modSet.Name) || !Get().Any(x => x.Name == modSet.Name))
                {
                    Items.Add(modSet.Id, modSet);
                    foreach (var mod in modSet.Mods)
                        Graph?.Context?.AddMod(mod);

                    return true;
                }
            }

            return false;
        }

        public void Remove(Guid modSetId)
        {
            var existing = Get().FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                Graph?.Context?.RemoveMods(existing.Mods.ToArray());
                Items.Remove(existing.Id);
            }
        }

        public void Remove(string name)
        {
            var existing = Get().FirstOrDefault(x => x.Name == name);
            if (existing != null)
            {
                Graph?.Context?.RemoveMods(existing.Mods.ToArray());
                Items.Remove(existing.Id);
            }
        }

        public override void OnTurnChanged(int turn)
        {
            foreach (var modSet in Get())
                modSet.OnTurnChanged(turn);
        }

        public override void OnBeginEncounter()
        {
            foreach (var modSet in Get())
                modSet.OnBeginEncounter();
        }

        public override void OnEndEncounter()
        {
            var toRemove = new List<ModSet>();
            foreach (var modSet in Get())
            {
                modSet.OnEndEncounter();
                if (modSet.GetExpiry(Graph) == ModExpiry.Expired)
                    toRemove.Add(modSet);
            }

            foreach (var remove in toRemove)
                Items.Remove(remove.Id);
        }
    }
}
