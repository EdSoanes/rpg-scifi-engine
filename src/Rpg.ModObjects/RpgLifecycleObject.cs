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
            Lifespan.SetStartTime(Graph.Time.Current);
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
            ExpiredTime = Graph.Time.Current;
            CalculateExpiry();
            Expiry = Lifespan.GetExpiry(Graph.Time.Current);
        }

        protected virtual void CalculateExpiry()
        {
            if (ExpiredTime != null)
                Expiry = new SpanOfTime(
                    new PointInTime(PointInTimeType.BeforeTime),
                    ExpiredTime.Value
                ).GetExpiry(Graph.Time.Current);
            else
                Expiry = Lifespan.GetExpiry(Graph!.Time.Current);

            if (Expiry == LifecycleExpiry.Expired && !Graph.Time.Current.IsEncounterTime)
                Expiry = LifecycleExpiry.Destroyed;
        }
    }
}
