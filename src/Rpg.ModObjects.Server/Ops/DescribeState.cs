using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class DescribeState
    {
        [JsonProperty] public string EntityId { get; set; }
        [JsonProperty] public string State {  get; set; }
    }
}
