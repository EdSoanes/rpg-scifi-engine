using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class DescribeProp
    {
        [JsonProperty] public string EntityId { get; set; }
        [JsonProperty] public string Prop {  get; set; }
    }
}
