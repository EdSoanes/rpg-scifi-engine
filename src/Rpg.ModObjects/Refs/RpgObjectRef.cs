using Newtonsoft.Json;

namespace Rpg.ModObjects.Refs
{
    public class RpgObjectRef<T> : RpgRef<string>
        where T : RpgObject
    {
        [JsonProperty] public string EntityId { get; private set; }
        [JsonProperty] private string? ChildEntityId { get; set; }

        public T? Object
        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            EntityId = entity!.Id;
        }
    }
}
