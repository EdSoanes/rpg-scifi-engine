using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Rpg.ModObjects.Stores
{
    public abstract class ModBaseStore<TKey, TVal> : ITemporal, INotifyCollectionChanged
        where TKey : notnull
        where TVal : class
    {
        protected RpgGraph Graph { get; set; }
        protected string EntityId { get; set; }

        [JsonProperty] protected Dictionary<TKey, TVal> Items { get; set; } = new Dictionary<TKey, TVal>();

        public TVal? this[TKey key]
        {
            get
            {
                if (Items.ContainsKey(key))
                    return Items[key];

                return null;
            }
            set
            {
                Add(key, value);
            }
        }

        public TKey[] Keys { get => Items.Keys.ToArray(); }

        public ModBaseStore(string entityId)
            => EntityId = entityId;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));


        public bool Add(TKey key, TVal? val)
        {
            if (val != null && !Contains(val) && !Contains(key))
            {
                Items.Add(key, val);
                CallCollectionChanged(NotifyCollectionChangedAction.Add);
                return true;
            }

            return false;
        }

        public bool Remove(TKey key)
        {
            if (Contains(key))
            {
                Items.Remove(key);
                CallCollectionChanged(NotifyCollectionChangedAction.Remove);
                return true;
            }

            return false;
        }

        public TVal[] Get()
            => Items.Values.ToArray();

        public bool Contains(TKey key)
            => Items.ContainsKey(key);

        public bool Contains(TVal val)
            => Items.Values.Contains(val);

        public virtual void OnGraphCreating(RpgGraph graph, RpgObject entity)
        {
            Graph = graph;
            EntityId = entity!.Id;
        }

        public virtual void OnObjectsCreating() { }
        public virtual void OnBeforeUpdate(RpgGraph graph) { }
        public virtual void OnAfterUpdate(RpgGraph graph) { }
    }
}
