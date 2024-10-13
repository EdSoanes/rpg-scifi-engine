using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityComplete
    {
        [JsonProperty] public string ActivityId { get; init; }
    }
}
