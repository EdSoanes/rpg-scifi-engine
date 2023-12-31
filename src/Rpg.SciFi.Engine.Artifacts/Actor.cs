using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Actor : Artifact
    {
        [JsonConstructor] public Actor() { }

        [JsonProperty] public TurnPoints Turns { get; private set; } = new TurnPoints();
    }
}
