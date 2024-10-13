using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class Describe
    {
        [JsonProperty] public string EntityId { get; set; }
        [JsonProperty] public string Prop {  get; set; }
    }
}
