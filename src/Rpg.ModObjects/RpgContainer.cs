using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgContainer : RpgComponent
    { 
        [JsonProperty] internal List<string> ContainerStore { get; private set; } = new List<string>();

        [JsonProperty] private List<RpgObject> PreAddedContents { get; set; } = new();
        [JsonProperty] private string? _preAddedContents { get; set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public RpgContainer(string entityId, string name)
            : base(entityId, name)
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
                PreAddedContents.Add(obj);
            else
            {
                Graph.AddEntity(obj);
                ContainerStore.Add(obj.Id);
            }

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
            foreach (var preAdded in GetPreAddedContents())
            {
                preAdded.OnBeforeTime(graph, preAdded);
                graph.AddEntity(preAdded);
            }

            base.OnBeforeTime(graph, entity);
        }

        private IEnumerable<RpgObject> GetPreAddedContents()
        {
            var res = new List<RpgObject>();
            res.AddRange(PreAddedContents);
            if (!string.IsNullOrEmpty(_preAddedContents))
            {
                var contents = RpgSerializer.Deserialize<RpgObject[]>(_preAddedContents);
                res.AddRange(contents);
            }

            _preAddedContents = null;
            PreAddedContents.Clear();

            foreach (var preAdded in res)
                if (!ContainerStore.Contains(preAdded.Id))
                    ContainerStore.Add(preAdded.Id);

            return res.SelectMany(x => x.Traverse());
        }
    }
}
