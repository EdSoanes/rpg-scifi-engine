using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class InitiateAction
    {
        [JsonProperty] public string InitiatorId { get; init; }
        [JsonProperty] public string ActionTemplateOwnerId { get; init; }
        [JsonProperty] public string ActionTemplateName { get; init; }
    }
}
