using Newtonsoft.Json;
using Rpg.ModObjects.Activities;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActionStepArgs
    {
        [JsonProperty] public string ActivityId { get; init; }
        [JsonProperty] public ActionStep ActionStep { get; set; }
    }
}
