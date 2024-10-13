using Rpg.ModObjects.Time;
using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using System.Collections.Specialized;

namespace Rpg.ModObjects
{
    public class RpgContainer : RpgComponent
    {
        public RpgObjectCollection Contents { get; private set; }

        [JsonConstructor] protected RpgContainer() 
            : base() { }

        public RpgContainer(string name, int maxItems = int.MaxValue)
            : base(name)
        {
            Contents = new RpgObjectCollection(Id, nameof(Contents));
            Contents.MaxItems = maxItems;
        }

        public T? Get<T>(string entityId)
            where T : RpgObject
                => Contents.FirstOrDefault(x => x.Id == entityId) as T;

        public IEnumerable<T> Get<T>(Func<T, bool>? filterFunc = null)
            where T : RpgObject
                => Contents.Where(x => (x is T e) && (filterFunc?.Invoke(e) ?? true))
                    .Cast<T>();

        public bool Contains(RpgEntity obj)
            => Contents.Any(x => x.Id == obj.Id);

        public bool Contains(string entityId)
            => Contents.Any(x => x.Id == entityId);

        public bool Add(RpgEntity obj)
        {
            if (Contains(obj))
                return false;

            Contents.Add(obj);
            return true;
        }

        public bool Remove(RpgEntity obj)
            => Contents.Remove(obj);

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Contents.OnCreating(graph, this);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            Contents.OnRestoring(graph, this);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Contents.OnTimeBegins();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            Contents.OnStartLifecycle();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            Contents.OnUpdateLifecycle();
            return Expiry;
        }
    }
}
