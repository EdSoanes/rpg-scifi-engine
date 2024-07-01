using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Attributes;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgContainer : RpgEntity
    { 
        [JsonProperty]
        [TextUI(Ignore = true)]
        public string EntityId { get; private set; }
    
        [JsonProperty] internal RpgEntityStore ContainerStore { get; private set; }
        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        public RpgContainer(string name)
            : base(name)
        {
            ContainerStore = new RpgEntityStore(Id);
        }

        public bool Contains(RpgEntity obj)
            => Contains(obj.Id);

        public bool Contains(string entityId)
        {
            foreach (var store in ContainerStore.Get())
                if (store.Any(x => x.Id == entityId))
                    return true;

            return false;
        }

        public string? ContainedInStore(RpgEntity entity)
        {
            foreach (var storeName in ContainerStore.Keys)
                if (ContainerStore[storeName]!.Any(x => x.Id == entity.Id))
                    return storeName;

            return null;
        }

        public bool AddToStore(string storeName, RpgEntity obj)
        {
            if (Contains(obj))
                return false;

            var store = ContainerStore[storeName];
            if (store == null)
            {
                store = new List<RpgEntity>();
                ContainerStore[storeName] = store;
            }

            store.Add(obj);
            Graph?.AddEntity(obj);

            CallCollectionChanged(NotifyCollectionChangedAction.Add);

            return true;
        }

        public bool RemoveFromStore(RpgEntity obj)
        {
            var storeName = ContainedInStore(obj);
            if (!string.IsNullOrEmpty(storeName))
            {
                var toRemove = ContainerStore[storeName]!.Single(x => x.Id == obj.Id);
                ContainerStore[storeName]!.Remove(toRemove);
                CallCollectionChanged(NotifyCollectionChangedAction.Remove);

                return true;
            }

            return false;
        }

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            this.EntityId ??= entity!.Id;
        }
    }
}
