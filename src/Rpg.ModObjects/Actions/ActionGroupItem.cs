using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Actions
{
    public class ActionGroupItem
    {
        [JsonInclude] public string OwnerArchetype { get; private init; }
        [JsonInclude] public string ActionName { get; private init; }
        [JsonInclude] public bool Optional { get; private init; }

        public ActionGroupItem(string ownerArchetype, string actionName, bool optional = true)
        {
            OwnerArchetype = ownerArchetype;
            ActionName = actionName;
            Optional = optional;
        }
    }
}
