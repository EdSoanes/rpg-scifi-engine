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
    
        [JsonProperty] internal List<string> ContainerStore { get; private set; } = new List<string>();
        
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public RpgContainer(string name)
            : base(name)
        {
        }

        public T? GetById<T>(string entityId)
            where T : RpgObject
        {
            if (Contains(entityId))
            {
                var obj = Graph!.GetEntity<T>(entityId);
                return obj;
            }

            return null;
        }

        public IEnumerable<T> Get<T>(Func<T, bool>? filterFunc = null)
            where T : RpgObject
                => ContainerStore
                    .Select(x => Graph!.GetEntity<T>(x))
                    .Where(x => x != null && (filterFunc?.Invoke(x) ?? true))
                    .Cast<T>();

        public bool Contains(RpgEntity obj)
            => Contains(obj.Id);

        public bool Contains(string entityId)
            => ContainerStore.Contains(entityId);

        public bool Add(RpgEntity obj)
        {
            if (Contains(obj))
                return false;

            if (Graph == null)
                RpgGraph.PreAddEntity(obj);
            else if (Graph.GetEntity(obj.Id) == null)
                Graph.AddEntity(obj);

            ContainerStore.Add(obj.Id);
            CallCollectionChanged(NotifyCollectionChangedAction.Add);

            return true;
        }

        public bool Remove(RpgEntity obj)
        {
            if (!Contains(obj))
                return false;

            ContainerStore.Remove(obj.Id);
            CallCollectionChanged(NotifyCollectionChangedAction.Remove);

            return true;
        }

        protected void CallCollectionChanged(NotifyCollectionChangedAction action)
            => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            EntityId ??= entity!.Id;
        }
    }
}
