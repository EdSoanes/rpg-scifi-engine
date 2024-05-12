using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSet : ITemporal
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string? Name { get; set; }
        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] private ModDuration Duration { get; set; } = new ModDuration();
        [JsonProperty] private Guid[] ModIds { get; set; } = new Guid[0];

        [JsonConstructor] private ModSet() { }

        public ModSet(ModDuration duration)
        {
            Duration = duration;
        }

        public ModSet(ModDuration duration, Mod[] mods)
            : this(duration)
        {
            Add(mods);
        }

        public void Add(params Mod[] mods)
        {
            Mods.AddRange(mods);
            ModIds = Mods.Select(x => x.Id).ToArray();

            SetModDuration();
        }

        private void SetModDuration()
        {
            foreach (var mod in Mods.Where(x => x.Duration.Type == ModDurationType.External))
                mod.Duration.SetDuration(Duration.StartTurn, Duration.EndTurn);
        }

        public ModExpiry GetExpiry(int turn)
            => Duration.GetExpiry(turn);

        public void SetExpired()
        {
            Duration.SetExpired();
            SetModDuration();
        }

        public void SetPending(int turn)
        {
            Duration.SetPending(turn);
            SetModDuration();
        }

        public void SetActive()
        {
            Duration.SetActive();
            SetModDuration();
        }

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null) { }
        public void OnGraphCreated(ModGraph graph)
        {
            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => ModIds.Contains(x.Id)).ToList();
        }

        public void OnTurnChanged(int turn) { }
        public void OnBeginEncounter() { }

        public void OnEndEncounter()
        {
            var toRemove = new List<Mod>();

            foreach (var mod in Mods)
            {
                var expiry = mod.Duration.GetExpiry(0);
                if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(0))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
            {
                Mods.Remove(mod);
                ModIds = Mods.Select(x => x.Id).ToArray();
            }
        }
    }
}
