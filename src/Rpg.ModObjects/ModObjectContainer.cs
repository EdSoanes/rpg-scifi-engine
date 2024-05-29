using Newtonsoft.Json;
using Rpg.ModObjects.Stores;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class ModObjectContainer : RpgObject, INotifyCollectionChanged
    {
        [JsonProperty] protected ModObjectStore ObjectStore { get; private set; } = new ModObjectStore();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        [JsonConstructor] private ModObjectContainer()
            : base() { }

        public ModObjectContainer(string name)
            : base()
                => Name = name;

        public bool Contains(RpgObject obj)
            => ObjectStore.Contains(obj.Id);

        public bool Add(RpgObject obj)
        {
            if (!ObjectStore.Contains(obj.Id))
            {
                ObjectStore.Add(obj.Id, obj);

                if (Graph?.AddEntity(obj) ?? false)
                    obj.OnGraphCreating(Graph);

                CallCollectionChanged(NotifyCollectionChangedAction.Add);
                return true;
            }

            return false;
        }

        public bool Remove(RpgObject obj)
        {
            if (ObjectStore.Contains(obj.Id))
            {
                ObjectStore.Remove(obj.Id);
                CallCollectionChanged(NotifyCollectionChangedAction.Remove);
                return true;
            }

            return false;
        }

        public bool TransferTo(RpgObject obj, ModObjectContainer target)
        {
            return Contains(obj) 
                && !target.Contains(obj)
                && Remove(obj) 
                && target.Add(obj);
        }

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }
}
