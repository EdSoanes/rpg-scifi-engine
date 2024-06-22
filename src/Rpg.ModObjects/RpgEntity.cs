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
        [JsonProperty] public StateStore StateStore { get; private set; }
        [JsonProperty] public RpgActionStore ActionStore { get; private set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public RpgEntity()
            : base() 
        {
            EntityStore = new RpgEntityStore(Id);
            StateStore = new StateStore(Id);
            ActionStore = new RpgActionStore(Id);
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        public bool IsStateOn(string state)
            => (StateStore[state]?.Expiry ?? LifecycleExpiry.Expired) == LifecycleExpiry.Active;

        public bool SetStateOn(string state)
            => StateStore[state]?.On() ?? false;

        public bool SetStateOff(string state)
            => StateStore[state]?.Off() ?? false;

        public State? GetState(string state)
            => StateStore[state];

        public State[] GetStates()
            => StateStore.Get();


        public bool IsActionEnabled<TOwner>(string action, RpgEntity initiator)
            where TOwner : RpgEntity
                => GetAction(action)?.IsEnabled<TOwner>((this as TOwner)!, initiator) ?? false;

        public Actions.Action? GetAction(string action)
            => ActionStore[action];

        public Actions.Action[] GetActions()
            => ActionStore.Get();


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

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);

            var actions = this.CreateActions();
            ActionStore.Add(actions);
            ActionStore.OnBeginningOfTime(graph, entity);

            var states = this.CreateStates()
                .Union(this.CreateStateActions(actions))
                .ToArray();

            StateStore.Add(states);
            StateStore.OnBeginningOfTime(graph, entity);
        }
    }
}
