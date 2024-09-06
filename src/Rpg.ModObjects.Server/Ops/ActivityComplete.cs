using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActivityComplete
    {
        [JsonInclude] public string ActivityId { get; init; }
    }
}
