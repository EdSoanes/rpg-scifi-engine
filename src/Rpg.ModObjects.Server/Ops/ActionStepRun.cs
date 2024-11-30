using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection.Args;

namespace Rpg.ModObjects.Server.Ops
{
    public class ActionStepRun
    {
        [JsonProperty] public string ActivityId { get; init; }
        [JsonProperty] public RpgArg[] Args { get; init; }
    }
}
