using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints
    {
        [JsonProperty] public virtual int BaseStrength { get; protected set; }
        [JsonProperty] public virtual int BaseDexterity { get; protected set; }
        [JsonProperty] public virtual int BaseIntelligence { get; protected set; }
    }
}
