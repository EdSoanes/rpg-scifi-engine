using Newtonsoft.Json;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgPropertyDataObject : IRpgPropertyData
    {
        private MetaProperty? _metaProperty;

        [JsonProperty] public string ObjectId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public RpgPropertyType PropType { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public bool IsVirtual { get; private set; }
        [JsonProperty] public List<RpgObjectRef> Refs { get; private set; } = new();

        [JsonConstructor] private RpgPropertyDataObject() { }

        public RpgPropertyDataObject(string objectId, MetaProperty metaProperty)
        {
            ObjectId = objectId;
            Prop = metaProperty.Prop;
            PropType = metaProperty.PropertyType;
            IsNullable = metaProperty.IsNullable;
        }

        public RpgPropertyDataObject(string objectId, string prop, RpgPropertyType propType, bool isVirtual)
        {
            ObjectId = objectId;
            Prop = prop;
            PropType = propType;
            IsNullable = true;
            IsVirtual = isVirtual;
        }

        public RpgProperty GetProperty(RpgGraph graph)
        {
            var rpgProperty = _metaProperty?.CloneAsProperty() ?? new RpgProperty
            {
                Prop = Prop,
                Editor = PropType == RpgPropertyType.Child ? EditorType.Child : EditorType.Children,
                DisplayName = Prop,
            };

            rpgProperty.IsNullable = IsNullable;
            rpgProperty.ObjectId = ObjectId;
            rpgProperty.ChildObjectIds = Refs
                .Where(x => x.Expiry == LifecycleExpiry.Active)
                .Select(x => x.ChildObjectId)
                .ToArray();

            return rpgProperty;
        }

        public T? GetValue<T>(RpgGraph graph)
        {
            var refs = Refs
                .Where(x => x.Expiry == LifecycleExpiry.Active)
                .Select(x => graph.GetObject(x.ChildObjectId))
                .Where(x => x != null);

            if (PropType == RpgPropertyType.Child)
            {
                var val = refs.FirstOrDefault();
                return val is T ? (T)(object)val : default;
            }

            return refs is T
                ? (T)refs
                : default;
        }

        public void Expire(TimePoint expiryTime) { }

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
            _metaProperty = graph.GetMetaProperty(ObjectId, Prop);

            if (obj == null) return;
            if (PropType == RpgPropertyType.Child)
            {
                var child = graph.GetPropertyValue<RpgObject>(obj, Prop);
                if (child != null)
                    Refs.Add(new RpgObjectRef(obj.Id, child.Id, TimePointType.TimeBegins, TimePointType.TimeEnds));
            }
            else if (PropType == RpgPropertyType.Children)
            {
                var children = graph.GetPropertyValue<ICollection<RpgObject>>(obj, Prop);
                if (children != null && children.Any())
                    Refs.AddRange(children.Select(x => new RpgObjectRef(obj.Id, x.Id, TimePointType.TimeBegins, TimePointType.TimeEnds)));
            }
        }

        public void OnRestoring(RpgGraph graph) 
        {
            _metaProperty = graph.GetMetaProperty(ObjectId, Prop);
        }

        public void OnCreatingVirtual(RpgGraph graph, object? value)
        {
            var obj = graph.GetObject(ObjectId)?.ResolvePropertyNameToObject(graph, Prop);
            if (obj == null) return;

            AddRefTo(obj.Id, obj.Start, obj.End);
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

        public void OnSyncProperty(RpgGraph graph, string prop)
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
                var oldList = graph.GetPropertyValue<ICollection<RpgObject>>(obj, Prop);
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

                    graph.SetPropertyValue(obj, Prop, oldList);
                }
            }
            else if (PropType == RpgPropertyType.Child)
            {
                var newChild = children.FirstOrDefault();
                var oldChild = graph.GetPropertyValue<RpgObject>(obj, Prop);
                if (newChild?.Id != oldChild?.Id)
                {
                    graph.SetPropertyValue(obj, Prop, newChild);
                }
            }
        }

        public override string ToString()
        {
            return $"{PropType}{(IsNullable ? "?" : "")} {Prop}";
        }
    }
}
