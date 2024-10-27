using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class DescribeModSet
    {
        [JsonProperty] public string EntityId { get; set; }
        [JsonProperty] public string ModSetId {  get; set; }
    }
}
