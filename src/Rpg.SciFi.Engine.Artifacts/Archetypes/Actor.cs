using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public abstract class Actor : Artifact
    {
        [JsonConstructor] public Actor() { }

        [JsonProperty] public TurnPoints Turns { get; private set; } = new TurnPoints();
    }
}
