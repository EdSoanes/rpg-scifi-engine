using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityAct
    {
        [JsonProperty] public string ActivityId { get; init; }
        [JsonProperty] public Dictionary<string, string?> Args { get; init; }
    }
}
