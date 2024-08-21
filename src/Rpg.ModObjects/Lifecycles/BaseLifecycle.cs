using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public abstract class BaseLifecycle : ILifecycle
    {
        protected RpgGraph Graph { get; private set; }

        [JsonProperty] public SpanOfTime Lifespan { get; protected set; } = new SpanOfTime();
        [JsonProperty] public PointInTime? ExpiredTime { get; protected set; }

        [JsonProperty] public LifecycleExpiry Expiry { get; protected set; } = LifecycleExpiry.Pending;

        public void SetExpired()
        {
            ExpiredTime ??= Graph.Time.Current;
        }

        public void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public virtual void OnTimeBegins()
        { }

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
    }
}
