using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModSet
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; } = nameof(ModSet);
        [JsonProperty] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ModDuration Duration { get; private set; } = new ModDuration();

        public void Expire(int turn)
        {
            foreach (var mod in Mods.Where(x => x.Duration.Type == ModDurationType.External))
                mod.Duration.Expire(turn);
        }
    }
}
