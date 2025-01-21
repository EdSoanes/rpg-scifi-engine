using Newtonsoft.Json;

namespace Rpg.Experimental.Meta
{
    public class MetaObject
    {
        [JsonProperty] public string Archetype { get; init; }
        [JsonProperty] public string[] Archetypes {  get; init; }
        [JsonProperty] public string? Icon { get; private set; }
        [JsonProperty] public List<MetaProperty> Properties { get; set; } = new List<MetaProperty>();

        [JsonProperty] public bool AllowedAsRoot { get; private set; }
        [JsonProperty] public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        [JsonProperty] public List<MetaAction> AllowedActions { get; set; } = new List<MetaAction>();
        [JsonProperty] public List<MetaState> AllowedStates { get; set; } = new List<MetaState>();
    }
}
