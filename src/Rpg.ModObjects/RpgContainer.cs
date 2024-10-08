﻿using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgContainer : RpgComponent
    {
        [JsonProperty] public RpgObject[] Contents { get => GetChildObjects(nameof(Contents)); }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        [JsonConstructor] public RpgContainer()
            : base() { }

        public RpgContainer(string name)
            : base(name)
        {
        }

        public T? Get<T>(string entityId)
            where T : RpgObject
                => GetChildObjects(nameof(Contents))
                    .FirstOrDefault(x => x.Id == entityId) as T;

        public IEnumerable<T> Get<T>(Func<T, bool>? filterFunc = null)
            where T : RpgObject
                => GetChildObjects(nameof(Contents))
                    .Where(x => (x is T e) && (filterFunc?.Invoke(e) ?? true))
                    .Cast<T>();

        public bool Contains(RpgEntity obj)
            => Contains(obj.Id);

        public bool Contains(string entityId)
            => GetProp(nameof(Contents))?.Contains(entityId) ?? false;

        public bool Add(RpgEntity obj)
        {
            if (Contains(obj))
                return false;

            AddChildObject(nameof(Contents), obj);
            CallCollectionChanged(NotifyCollectionChangedAction.Add);

            return true;
        }

        public bool Remove(RpgEntity obj)
        {
            if (!Contains(obj))
                return false;

            RemoveChildObject(nameof(Contents), obj);
            CallCollectionChanged(NotifyCollectionChangedAction.Remove);

            return true;
        }

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    }
}
