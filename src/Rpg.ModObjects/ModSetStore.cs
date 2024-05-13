using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSetStore : ITemporal
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonProperty] public Guid EntityId { get; set; }
        [JsonProperty] public List<ModSet> ModSets { get; private set; } = new List<ModSet>();

        public ModSetStore() { }

        public bool Add(ModSet modSet)
        {
            if (!ModSets.Contains(modSet))
            {
                if (string.IsNullOrEmpty(modSet.Name) || !ModSets.Any(x => x.Name == modSet.Name))
                {
                    ModSets.Add(modSet);
                    foreach (var mod in modSet.Mods)
                        Graph?.Context?.AddMod(mod);

                    return true;
                }
            }

            return false;
        }

        public ModSet[] All()
            => ModSets.ToArray();

        public void Remove(Guid modSetId)
        {
            var existing = ModSets.FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                Graph?.Context?.RemoveMods(existing.Mods.ToArray());
                ModSets.Remove(existing);
            }
        }

        public void Remove(string name)
        {
            var existing = ModSets.FirstOrDefault(x => x.Name == name);
            if (existing != null)
            {
                Graph?.Context?.RemoveMods(existing.Mods.ToArray());
                ModSets.Remove(existing);
            }
        }

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            EntityId = entity!.Id;
        }

        public void OnTurnChanged(int turn)
        {
            foreach (var modSet in ModSets)
                modSet.OnTurnChanged(turn);
        }

        public void OnBeginEncounter()
        {
            foreach (var modSet in ModSets)
                modSet.OnBeginEncounter();
        }

        public void OnEndEncounter()
        {
            var toRemove = new List<ModSet>();
            foreach (var modSet in ModSets)
            {
                modSet.OnEndEncounter();
                if (modSet.GetExpiry(ModDuration.EndEncounter) == ModExpiry.Expired)
                    toRemove.Add(modSet);
            }

            if (toRemove.Any())
                ModSets = ModSets.Except(toRemove).ToList();
        }
    }
}
