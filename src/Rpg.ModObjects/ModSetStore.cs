using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSetStore : ITemporal
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public Guid EntityId { get; set; }
        [JsonProperty] public List<ModSet> ModSets { get; private set; } = new List<ModSet>();

        public ModSetStore() { }

        public void Initialize(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void Add(ModSet modSet)
        {
            if (!ModSets.Contains(modSet))
                ModSets.Add(modSet);
        }

        public ModSet[] All()
            => ModSets.ToArray();

        public void Remove(Guid modSetId)
        {
            var existing = ModSets.FirstOrDefault(x => x.Id == modSetId);
            if (existing != null)
            {
                Graph?.Context?.PropStore.Remove(existing.Mods);
                ModSets.Remove(existing);
            }
        }

        public void OnTurnChanged(int turn)
        {
            foreach (var modSet in ModSets)
                modSet.OnTurnChanged(turn);
        }

        public void OnEncounterStarted()
        {
            foreach (var modSet in ModSets)
                modSet.OnEncounterStarted();
        }

        public void OnEncounterEnded()
        {
            var toRemove = new List<ModSet>();
            foreach (var modSet in ModSets)
            {
                modSet.OnEncounterEnded();
                if (modSet.GetExpiry(ModDuration.EndEncounter) == ModExpiry.Expired)
                    toRemove.Add(modSet);
            }

            if (toRemove.Any())
                ModSets = ModSets.Except(toRemove).ToList();
        }
    }
}
