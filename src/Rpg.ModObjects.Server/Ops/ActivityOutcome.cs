using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityOutcome
    {
        [JsonInclude] public string ActivityId { get; init; }
        [JsonInclude] public Dictionary<string, string?> Args { get; init; }
    }
}
