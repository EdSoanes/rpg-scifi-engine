using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Components
{


    public class TurnPoints
    {
        [JsonProperty] public Score Action { get; protected set; } = new Score(nameof(Action));
        [JsonProperty] public Score Exertion { get; protected set; } = new Score(nameof(Exertion));
        [JsonProperty] public Score Focus { get; protected set; } = new Score(nameof(Focus));
    }
}
