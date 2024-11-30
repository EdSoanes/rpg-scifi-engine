using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityCreate
    {
        [JsonProperty] public string InitiatorId { get; init; }
        [JsonProperty] public string OwnerId { get; init; }
        [JsonProperty] public string ActionTemplate { get; init; }
    }
}
