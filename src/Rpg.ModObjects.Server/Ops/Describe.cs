using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Ops
{
    public class Describe
    {
        [JsonInclude] public string EntityId { get; set; }
        [JsonInclude] public string Prop {  get; set; }
    }
}
