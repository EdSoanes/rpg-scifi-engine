using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints
    {
        [JsonProperty] public virtual Score Strength { get; protected set; } = new Score(nameof(Strength));
        [JsonProperty] public virtual Score Dexterity { get; protected set; } = new Score(nameof(Dexterity));
        [JsonProperty] public virtual Score Intelligence { get; protected set; } = new Score(nameof(Intelligence));
    }
}
