using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
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
            Graph?.AddEntity(obj);

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

    public abstract class RpgEntity<T> : RpgEntity
    where T : RpgEntity
    {
        [JsonProperty] public StateStore<T> StateStore { get; private set; }
        [JsonProperty] public RpgActionStore<T> ActionStore { get; private set; }

        public RpgEntity()
        {
            StateStore = new StateStore<T>(Id);
            ActionStore = new RpgActionStore<T>(Id);
        }

        public bool IsStateOn(string state)
            => (StateStore[state]?.Expiry ?? LifecycleExpiry.Expired) == LifecycleExpiry.Active;

        public bool SetStateOn(string state)
            => StateStore[state]?.On() ?? false;

        public bool SetStateOff(string state)
            => StateStore[state]?.Off() ?? false;

        public StateModification<T>? GetState(string state)
            => StateStore[state];

        public bool IsActionEnabled(string action, RpgEntity initiator)
            => GetAction(action)?.IsEnabled((this as T)!, initiator) ?? false;

        public ActionModification<T>? GetAction(string action)
            => ActionStore[action];
    }
}
