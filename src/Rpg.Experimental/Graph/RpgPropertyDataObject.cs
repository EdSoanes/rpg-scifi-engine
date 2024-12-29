using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgPropertyDataObject : IRpgPropertyData
    {
        [JsonProperty] public string ObjectId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public RpgPropertyType PropType { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public List<RpgObjectRef> Refs { get; private set; } = new();

        [JsonConstructor] private RpgPropertyDataObject() { }

        public RpgPropertyDataObject(string objectId, string prop, RpgPropertyType propType)
        {
            ObjectId = objectId;
            Prop = prop;
            PropType = propType;
            IsNullable = true;
        }

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);
        public void Expire(RpgGraph graph, TimePoint expiryTime) { }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            var toExpire = Refs
                .Where(x => x.ChildObjectId == objectId && x.Expiry == LifecycleExpiry.Active)
                .ToArray();

            foreach (var propRef in toExpire)
                propRef.Expire(graph, expiryTime);

            if (toExpire.Any())
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
        }

        public void AddRefTo(string objectId, TimePoint start, TimePoint end)
        {
            if (PropType == RpgPropertyType.Child && !Refs.Any(x => x.Expiry == LifecycleExpiry.Active))
            {
                Refs.Add(new RpgObjectRef(ObjectId, objectId, start, end));
            }
            else if (PropType == RpgPropertyType.Children && !Refs.Any(x => x.ChildObjectId == objectId && x.Expiry == LifecycleExpiry.Active))
            {
                Refs.Add(new RpgObjectRef(ObjectId, objectId, start, end));
            }
        }

        public void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            if (obj == null) return;
            if (PropType == RpgPropertyType.Child)
            {
                var child = obj.Value<RpgObject>(Prop);
                if (child != null)
                    Refs.Add(new RpgObjectRef(obj.Id, child.Id, TimePointType.TimeBegins, TimePointType.TimeEnds));
            }
            else if (PropType == RpgPropertyType.Children)
            {
                var children = obj.Value<IEnumerable<RpgObject>>(Prop);
                if (children != null && children.Any())
                    Refs.AddRange(children.Select(x => new RpgObjectRef(obj.Id, x.Id, TimePointType.TimeBegins, TimePointType.TimeEnds)));
            }
        }

        public void OnTimeEvent(RpgGraph graph)
        {
            var updated = false;
            foreach (var objRef in Refs)
            {
                var oldExpiry = objRef.Expiry;
                objRef.OnTimeEvent(graph);
                updated |= oldExpiry != objRef.Expiry;
            }

            Refs = Refs
                .Where(x => x.Expiry != LifecycleExpiry.Destroyed)
                .ToList();

            if (updated)
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
        }

        public void OnSyncProperty(RpgGraph graph)
        {
            var obj = graph.GetObject(ObjectId)!;
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.Id != ObjectId)
                throw new ArgumentException($"Invalid object id {obj.Id}", "obj");

            var refs = Refs.Where(x => x.Expiry == LifecycleExpiry.Active);
            var children = refs
                .Select(x => graph.GetObject(x.ChildObjectId))
                .Where(x => x != null)
                .Cast<RpgObject>()
                .ToList();

            if (PropType == RpgPropertyType.Children)
            {
                var oldList = obj.Value<ICollection<RpgObject>>(Prop);
                if (oldList == null)
                {
                    var propInfo = obj.GetType().GetProperty(Prop)!;
                    oldList = (ICollection<RpgObject>)Activator.CreateInstance(propInfo.PropertyType)!;
                }

                var oldIds = oldList.Select(x => x.Id).Distinct();
                var newIds = children.Select(x => x.Id).Distinct();
                if (oldIds.Except(newIds).Any() || oldIds.Count() != newIds.Count())
                {
                    oldList.Clear();
                    foreach (var child in children)
                        oldList.Add(child);

                    obj.SetPropertyValue(Prop, oldList);
                    graph.ChangeTracker.PropUpdated(ObjectId, Prop);
                }
            }
            else if (PropType == RpgPropertyType.Child)
            {
                var newChild = children.FirstOrDefault();
                var oldChild = obj.Value<RpgObject>(Prop);
                if (newChild?.Id != oldChild?.Id)
                {
                    obj.SetPropertyValue<RpgObject>(Prop, newChild);
                    graph.ChangeTracker.PropUpdated(ObjectId, Prop);
                }
            }
        }

        public override string ToString()
        {
            return $"{PropType}{(IsNullable ? "?" : "")} {Prop}";
        }
    }
}
