using Newtonsoft.Json;
using Rpg.Experimental.Reflection;

namespace Rpg.Experimental.System
{
    public class MetaAction
    {
        [JsonProperty] public string Name { get; init; }
        [JsonProperty] public string OwnerArchetype { get; init; }

        [JsonProperty] public RpgMethod<RpgAction, bool>? Cost { get; init; }
        [JsonProperty] public RpgMethod<RpgAction, bool>? Perform { get; init; }
        [JsonProperty] public RpgMethod<RpgAction, bool> Outcome { get; init; }

        [JsonConstructor] public MetaAction() { }

        public override string ToString()
        {
            return $"{Name} ({OwnerArchetype})";
        }
    }
}
