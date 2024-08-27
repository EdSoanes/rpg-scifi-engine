using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public class RpgLifecycleObject
    {
        protected RpgGraph Graph { get; private set; }
        [JsonProperty] protected SpanOfTime Lifespan { get; set; } = new SpanOfTime();
        [JsonProperty] protected PointInTime? ExpiredTime { get; private set; }

        public LifecycleExpiry Expiry { get; set; } = LifecycleExpiry.Unset;

        public virtual void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            if (graph == null)
                throw new InvalidOperationException("Cannot set RpgLifecycleObject.OnCreating.Graph to null");

            if (Graph == null)
                Graph = graph;
        }

        public virtual void OnRestoring(RpgGraph graph)
        {
            if (graph == null)
                throw new InvalidOperationException("Cannot set RpgLifecycleObject.OnRestoring.Graph to null");

            if (Graph == null)
                Graph = graph;
        }

        public virtual void OnTimeBegins()
        {
        }

        public virtual LifecycleExpiry OnStartLifecycle()
        {
            Lifespan.SetStartTime(Graph.Time.Now);
            CalculateExpiry();
            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle()
        {
            CalculateExpiry();
            return Expiry;
        }

        public virtual void SetExpired()
        {
            ExpiredTime = Graph.Time.Now;
            CalculateExpiry();
            Expiry = Lifespan.GetExpiry(Graph.Time.Now);
        }

        protected virtual void CalculateExpiry()
        {
            if (ExpiredTime != null)
                Expiry = new SpanOfTime(PointInTimeType.BeforeTime, ExpiredTime.Value).GetExpiry(Graph.Time.Now);
            else
                Expiry = Lifespan.GetExpiry(Graph!.Time.Now);

            if (Expiry == LifecycleExpiry.Expired && !Graph.Time.Now.IsEncounterTime)
                Expiry = LifecycleExpiry.Destroyed;
        }
    }
}
