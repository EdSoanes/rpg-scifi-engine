using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgEntity : RpgObject, INotifyCollectionChanged
    {
        [JsonProperty] protected RpgEntityStore EntityStore { get; private set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        [JsonConstructor] protected RpgEntity()
            : base() { }

        public RpgEntity(string name)
            : base()
        {
            Name = name;
            EntityStore = new RpgEntityStore(Id);
        }

        public bool Contains(RpgEntity obj)
            => Contains(obj.Id);

        public bool Contains(string entityId)
        {
            foreach (var store in EntityStore.Get())
                if (store.Any(x => x.Id == entityId))
                    return true;

            return false;
        }

        public string? ContainedInStore(RpgEntity entity)
        {
            foreach (var storeName in EntityStore.Keys)
                if (EntityStore[storeName]!.Any(x => x.Id == entity.Id))
                    return storeName;

            return null;
        }

        public bool AddToStore(string storeName, RpgEntity obj)
        {
            if (Contains(obj)) 
                return false;

            var store = EntityStore[storeName];
            if (store == null)
            {
                store = new List<RpgEntity>();
                EntityStore[storeName] = store;
            }

            store.Add(obj);
            if (Graph?.AddEntity(obj) ?? false)
                obj.OnGraphCreating(Graph);

            CallCollectionChanged(NotifyCollectionChangedAction.Add);

            return true;
        }

        public bool RemoveFromStore(RpgEntity obj)
        {
            var storeName = ContainedInStore(obj);
            if (!string.IsNullOrEmpty(storeName))
            {
                var toRemove = EntityStore[storeName]!.Single(x => x.Id == obj.Id);
                EntityStore[storeName]!.Remove(toRemove);
                CallCollectionChanged(NotifyCollectionChangedAction.Remove);

                return true;
            }

            return false;
        }

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }
}
