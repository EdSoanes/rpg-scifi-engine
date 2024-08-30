using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Refs
{

    public sealed class RpgObjectRef : RpgRef<RpgObjectRef.ObjRef>
    {
        public class ObjRef
        {
            public string? SourceId { get; init; }
            public required RelationshipType RefType { get; init; }
        }

        [JsonProperty] public string TargetId { get; private set; }
        [JsonProperty] public RelationshipType RelType { get; private set; } = RelationshipType.Child;

        public RpgObject? GetSource()
            => Graph.GetObject(Get()?.SourceId);

        public T? GetSource<T>()
            where T : RpgObject
                => Graph.GetObject<T>(Get()?.SourceId);

        public void SetSource(RpgObject obj, SpanOfTime? lifespan = null)
            => Set(new ObjRef { SourceId = obj?.Id, RefType = this.RelType }, lifespan);

        public RpgObjectRef(string targetId, RelationshipType refType)
            : base()
        {
            TargetId = targetId;
            RelType = refType;
        }

        public RpgObjectRef(string targetId, RelationshipType refType, RpgGraph? graph)
            : base(graph)
        {
            TargetId = targetId;
            RelType = refType;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            TargetId = entity!.Id;
            base.OnCreating(graph, entity);
        }
    }
}
