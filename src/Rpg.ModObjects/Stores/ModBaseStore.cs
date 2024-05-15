using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Rpg.ModObjects.Stores
{
    public abstract class ModBaseStore<TKey, TVal> : ITemporal, INotifyCollectionChanged
        where TKey : notnull
        where TVal : class
    {
        protected ModGraph Graph { get; set; }
        protected Guid EntityId { get; set; }

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

        public virtual void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity!.Id;
        }

        public TVal[] Get()
            => Items.Values.ToArray();

        public bool Contains(TKey key)
            => Items.ContainsKey(key);

        public bool Contains(TVal val)
            => Items.Values.Contains(val);

        public abstract void OnBeginEncounter();
        public abstract void OnEndEncounter();
        public abstract void OnTurnChanged(int turn);
    }
}
