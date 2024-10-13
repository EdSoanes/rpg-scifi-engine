using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityCreateByTemplate
    {
        [JsonProperty] public string InitiatorId { get; init; }
        [JsonProperty] public string OwnerId { get; init; }
        [JsonProperty] public string ActivityTemplateName { get; init; }
    }
}
