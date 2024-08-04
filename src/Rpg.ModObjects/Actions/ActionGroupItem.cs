using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public class ActionGroupItem
    {
        [JsonProperty] public string OwnerArchetype { get; private init; }
        [JsonProperty] public string ActionName { get; private init; }
        [JsonProperty] public bool Optional { get; private init; }

        public ActionGroupItem(string ownerArchetype, string actionName, bool optional = true)
        {
            OwnerArchetype = ownerArchetype;
            ActionName = actionName;
            Optional = optional;
        }
    }
}
