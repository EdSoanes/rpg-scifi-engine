using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSet : ITemporal
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; } = nameof(ModSet);
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] private ModDuration Duration { get; set; } = new ModDuration();

        [JsonConstructor] private ModSet() { }

        public ModSet(ModDuration duration, Mod[] mods)
        {
            Duration = duration;
            Mods.AddRange(mods);
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

        public void OnTurnChanged(int turn)
        {
        }

        public void OnEncounterStarted()
        {
        }

        public void OnEncounterEnded()
        {
            var toRemove = new List<Mod>();

            foreach (var mod in Mods)
            {
                var expiry = mod.Duration.GetExpiry(0);
                if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(0))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
                Mods.Remove(mod);
        }
    }
}
