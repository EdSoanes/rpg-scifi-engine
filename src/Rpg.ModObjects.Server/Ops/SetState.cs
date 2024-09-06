using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class SetState
    {
        [JsonInclude] public string EntityId { get; set; }
        [JsonInclude] public string State {  get; set; }
        [JsonInclude] public bool On { get; set; }
    }
}
