using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSet
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; } = nameof(ModSet);
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ModDuration Duration { get; private set; } = new ModDuration();

        [JsonConstructor] private ModSet() { }

        public ModSet(ModDuration duration, Mod[] mods)
        {
            Duration = duration;
            Mods.AddRange(mods);
        }

        public void UpdatePropExpiry(int turn)
        {
            var expiry = Duration.GetExpiry(turn);
            if (expiry == ModExpiry.Expired)
                Mods.ForEach(mod => mod.Duration.SetExpired(turn));
            if (expiry == ModExpiry.Pending)
                Mods.ForEach(mod => mod.Duration.SetPending(turn));
            else
                Mods.ForEach(mod => mod.Duration.SetActive());
        }

        public void RemoveExpiredMods(int turn)
        {
            var toRemove = new List<Mod>();

            foreach (var mod in Mods)
            {
                var expiry = mod.Duration.GetExpiry(turn);
                if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(turn))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
                Mods.Remove(mod);
        }
    }
}
