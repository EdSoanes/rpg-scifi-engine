using Newtonsoft.Json;

namespace Rpg.ModObjects.Activities
{
    public sealed class ActionRef
    {
        [JsonProperty] public string ActivityId { get; private set; }
        [JsonProperty] public string ActionTemplateOwnerId { get; private set; }
        [JsonProperty] public string ActionTemplateName { get; private set; }
        [JsonProperty] public bool Optional { get; private set; }

        [JsonConstructor] private ActionRef() { }

        public ActionRef(string activityId, string actionOwnerId, string action, bool optional)
        {
            ActivityId = activityId;
            ActionTemplateOwnerId = actionOwnerId;
            ActionTemplateName = action;
            Optional = optional;
        }
    }
}
