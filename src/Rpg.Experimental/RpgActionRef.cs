using Newtonsoft.Json;

namespace Rpg.Experimental
{
    /// <summary>
    /// A reference to an action on an object, e.g. an action nominated to follow another in an activity
    /// </summary>
    public sealed class RpgActionRef
    {
        [JsonProperty] public string ActionOwnerId { get; private set; }
        [JsonProperty] public string ActionName { get; private set; }
        [JsonProperty] public bool Optional { get; private set; }

        [JsonConstructor] private RpgActionRef() { }

        public RpgActionRef(string actionOwnerId, string actionName, bool optional)
        {
            ActionOwnerId = actionOwnerId;
            ActionName = actionName;
            Optional = optional;
        }

        public override string ToString()
        {
            return $"{ActionOwnerId}.{ActionName}{(Optional ? " (optional)" : "")}";
        }
    }
}
