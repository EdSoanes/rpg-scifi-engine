using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using System.Collections;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects
{
    public class RpgObjectCollection : RpgLifecycleObject, IList<RpgObject>
    {
        private List<RpgObject>? _preAddedObjects = new();

        [JsonInclude] protected string ContainerName { get; set; }
        [JsonInclude] protected string EntityId { get; set; }

        protected RpgObject? Entity { get => Graph?.GetObject(EntityId); }
        protected Prop? EntityProp { get => Entity?.GetProp(ContainerName, RefType.Children); }

        public RpgObjectCollection() : base() { }

        public RpgObjectCollection(string entityId, string containerName)
            : base() 
        {
            EntityId = entityId;
            ContainerName = containerName;
        }

        public int Count 
            => Entity?.GetChildObjects(ContainerName).Count() ?? 0;

        public bool IsReadOnly => false;

        public RpgObject this[int index] 
        {
            get => Entity!.GetChildObjects(ContainerName)[index];
            set => throw new NotImplementedException();
        }

        public void Add(RpgObject item)
        {
            if (Entity != null)
                Entity.AddChildren(ContainerName, item);
            else if (!_preAddedObjects!.Any(x => x.Id == item.Id))
                _preAddedObjects.Add(item);
        }

        public void Clear()
        {
            EntityProp?.Refs?.Clear();
        }

        public bool Contains(RpgObject item)
        {
            if (EntityProp != null)
            {
                return EntityProp?
                    .Refs
                    .Any(x => x.Expiry == LifecycleExpiry.Active && x.EntityId == item.Id)
                    ?? false;

            }

            return _preAddedObjects?.Any(x => x.Id == item.Id) ?? false;
        }

        public void CopyTo(RpgObject[] array, int arrayIndex)
        {
            
        }

        public IEnumerator<RpgObject> GetEnumerator()
        {
            var objRefs = EntityProp?.Refs ?? Enumerable.Empty<PropObjRef>();
            foreach (var objRef in objRefs)
            {
                if (objRef.Expiry == LifecycleExpiry.Active)
                {
                    var obj = Graph!.GetObject(objRef.EntityId);
                    if (obj != null)
                        yield return obj;
                }
            }

            if (_preAddedObjects != null)
                foreach (var obj in _preAddedObjects)
                    yield return obj;
        }

        public bool Remove(RpgObject item)
        {
            if (Contains(item))
            {
                if (Entity != null)
                    Entity.RemoveChildren(ContainerName, item);
                else if (_preAddedObjects != null && _preAddedObjects.Contains(item))
                    _preAddedObjects.Remove(item);

                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public int IndexOf(RpgObject item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, RpgObject item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            if (Entity != null && _preAddedObjects != null)
            {
                Entity.AddChildren(ContainerName, _preAddedObjects.ToArray());
                _preAddedObjects = null;
            }
        }
    }
}
