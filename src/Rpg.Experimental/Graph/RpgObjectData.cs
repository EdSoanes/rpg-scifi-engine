using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public class RpgObjectData : ILifecycle
    {
        public string? ParentId { get; set; }
        public string ObjectId { get; set; }

        [JsonProperty] public List<IRpgPropertyData> Props { get; private set; } = new();

        [JsonConstructor] private RpgObjectData() { }

        public RpgObjectData(string objectId, string? parentId, IEnumerable<IRpgPropertyData> props)
        {
            ParentId = parentId;
            ObjectId = objectId;
            Props.AddRange(props);
        }

        public void Expire(TimePoint expiryTime) { }

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);

        public void Expire(RpgGraph graph, TimePoint expiryTime)
        {
            foreach (var objData in graph.ObjectData.Values)
                objData.ExpireRefsTo(graph, expiryTime, ObjectId);
        }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            if (ParentId == objectId)
                ParentId = null;

            foreach (var prop in Props)
                prop.ExpireRefsTo(graph, expiryTime, objectId);
        }

        public void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            foreach (var prop in Props)
                prop.OnCreating(graph, obj);
        }

        public void OnRestoring(RpgGraph graph) { }

        public void OnTimeEvent(RpgGraph graph)
        {
            foreach (var prop in Props)
                prop.OnTimeEvent(graph);
        }

        public void OnSyncProperty(RpgGraph graph, string prop)
            => GetPropData(prop)?.OnSyncProperty(graph, prop);

        public IRpgPropertyData? GetPropData(string prop)
            => Props.FirstOrDefault(x => x.Prop == prop);

        public T? GetPropData<T>(string prop)
            where T : class, IRpgPropertyData
        {
            var propData = Props.FirstOrDefault(x => x.Prop == prop);
            return propData as T;
        }
    }
}
