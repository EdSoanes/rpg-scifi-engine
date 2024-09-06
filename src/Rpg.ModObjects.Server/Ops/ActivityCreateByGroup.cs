using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityCreateByGroup
    {
        [JsonInclude] public string InitiatorId { get; init; }
        [JsonInclude] public string OwnerId { get; init; }
        [JsonInclude] public string ActionGroup { get; init; }
    }
}
