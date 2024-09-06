using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityCreate
    {
        [JsonInclude] public string InitiatorId { get; init; }
        [JsonInclude] public string OwnerId { get; init; }
        [JsonInclude] public string Action { get; init; }
    }
}
