using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public class RpgLifecycleObject : ILifecycle
    {
        protected RpgGraph Graph { get; private set; }
        [JsonProperty] protected SpanOfTime Lifespan { get; set; } = new SpanOfTime();
        [JsonProperty] protected PointInTime? ExpiredTime { get; private set; }

        public LifecycleExpiry Expiry { get; set; } = LifecycleExpiry.Unset;

        public virtual void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
        }

        public virtual void OnRestoring(RpgGraph graph)
        {
            Graph = graph;
        }

        public virtual void OnTimeBegins()
        {
        }

        public virtual LifecycleExpiry OnStartLifecycle()
        {
            Expiry = ExpiredTime != null && ExpiredTime <= Graph.Time.Current
                ? LifecycleExpiry.Expired
                : Lifespan.GetExpiry(Graph!.Time.Current);
 
            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle()
        {
            Expiry = ExpiredTime != null && ExpiredTime <= Graph.Time.Current
                ? LifecycleExpiry.Expired
                : Lifespan.GetExpiry(Graph!.Time.Current);

            return Expiry;
        }

        public virtual void SetExpired()
        {
            ExpiredTime = Graph.Time.Current;
            Expiry = Lifespan.GetExpiry(Graph.Time.Current);
        }
    }
}
