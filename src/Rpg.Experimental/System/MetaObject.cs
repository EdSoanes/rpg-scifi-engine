using Newtonsoft.Json;

namespace Rpg.Experimental.System
{
    public class MetaObject
    {
        [JsonProperty] public string Archetype { get; init; }
        [JsonProperty] public string[] Archetypes {  get; init; }

        /// <summary>The .net type of the object</summary>
        [JsonProperty] public string? QualifiedTypeName { get; init; }

        /// <summary>
        /// The values to author to create an object of this type. Null if the type cannot be created from
        /// authored values.
        /// </summary>
        [JsonProperty] public MetaTemplate? Template { get; init; }
        [JsonProperty] public string? Icon { get; private set; }
        [JsonProperty] public List<MetaProperty> Properties { get; set; } = new List<MetaProperty>();

        [JsonProperty] public bool AllowedAsRoot { get; private set; }
        [JsonProperty] public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        [JsonProperty] public List<MetaAction> AllowedActions { get; set; } = new List<MetaAction>();
        [JsonProperty] public List<MetaState> AllowedStates { get; set; } = new List<MetaState>();
    }
}
