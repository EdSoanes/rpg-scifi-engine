using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityCreateByGroup
    {
        [JsonProperty] public string InitiatorId { get; init; }
        [JsonProperty] public string OwnerId { get; init; }
        [JsonProperty] public string ActionGroup { get; init; }
    }
}
