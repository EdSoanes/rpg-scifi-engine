using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class RpgLifecycleObject : ILifecycle
    {
        protected RpgGraph Graph { get; set; }
        [JsonProperty] protected Lifespan Lifespan { get; set; } = new Lifespan();
        [JsonProperty] protected PointInTime? ExpiredTime { get; set; }
        [JsonProperty] public bool IsApplied { get; protected set; } = true;
        [JsonProperty] public bool IsDisabled { get; protected set; }

        public LifecycleExpiry Expiry { get; set; } = LifecycleExpiry.Unset;

        public RpgLifecycleObject() { }

        public void SetLifespan(RpgLifecycleObject fromObject)
        {
            Lifespan = fromObject.Lifespan;
        }

        public void SetLifespan(Lifespan lifespan)
        {
            Lifespan = lifespan;
        }

        public virtual void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            if (graph == null)
                throw new InvalidOperationException("Cannot set RpgLifecycleObject.OnCreating.Graph to null");

            if (Graph == null)
                Graph = graph;
        }

        public virtual void OnRestoring(RpgGraph graph, RpgObject? entity = null)
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
        }

        protected virtual void CalculateExpiry()
            => CalculateExpiry(null);

        protected void CalculateExpiry(string? syncedToId)
        {
            if (ExpiredTime != null)
                Expiry = new Lifespan(PointInTimeType.BeforeTime, ExpiredTime.Value).UpdateExpiry(Graph.Time.Now);
            else
                Expiry = Lifespan.UpdateExpiry(Graph!.Time.Now);

            if (Expiry == LifecycleExpiry.Expired && !Graph.Time.Now.IsEncounterTime)
                Expiry = LifecycleExpiry.Destroyed;

            if (Expiry == LifecycleExpiry.Active || Expiry == LifecycleExpiry.Suspended)
                Expiry = !IsDisabled && IsApplied
                    ? LifecycleExpiry.Active
                    : LifecycleExpiry.Suspended;

            var ownerExpiry = CalculateOwnerExpiry(syncedToId);
            if (ownerExpiry != LifecycleExpiry.Unset)
            {
                Expiry = ownerExpiry == LifecycleExpiry.Active && Expiry == LifecycleExpiry.Suspended
                    ? LifecycleExpiry.Suspended
                    : ownerExpiry;
            }
        }

        protected virtual LifecycleExpiry CalculateOwnerExpiry(string? ownerId)
        {
            if (Graph != null && ownerId != null)
            {
                var owner = Graph.GetObject(ownerId);
                if (owner != null)
                    return owner.Expiry;
            }

            return LifecycleExpiry.Unset;
        }
    }
}
