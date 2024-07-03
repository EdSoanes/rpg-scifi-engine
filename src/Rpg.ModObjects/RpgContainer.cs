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
        
        private List<RpgEntity> _preloadedEntities = new List<RpgEntity>();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public RpgContainer(string name)
            : base(name)
        {
        }

        public bool Contains(RpgEntity obj)
            => Contains(obj.Id);

        public bool Contains(string entityId)
            => ContainerStore.Contains(entityId);

        public bool Add(RpgEntity obj)
        {
            if (Contains(obj))
                return false;

            if (Graph == null)
                _preloadedEntities.Add(obj);
            else if (Graph.GetEntity(obj.Id) == null)
                Graph!.AddEntity(obj);

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
            foreach (var e in _preloadedEntities)
            {
                e.OnBeforeTime(graph, e);
                graph.AddEntity(e);
            }

            _preloadedEntities.Clear();

            base.OnBeforeTime(graph, entity);
            this.EntityId ??= entity!.Id;
        }
    }
}
