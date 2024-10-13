using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Ops
{
    public class SetState
    {
        [JsonProperty] public string EntityId { get; set; }
        [JsonProperty] public string State {  get; set; }
        [JsonProperty] public bool On { get; set; }
    }
}
