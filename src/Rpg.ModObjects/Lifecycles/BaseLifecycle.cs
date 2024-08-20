using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public abstract class BaseLifecycle : ILifecycle
    {
        [JsonProperty] public PointInTime? ExpiredTime { get; protected set; }
        [JsonProperty] public PointInTime StartTime { get; protected set; }
        [JsonProperty] public PointInTime EndTime { get; protected set; }
        [JsonProperty] public PointInTime CreatedTime { get; protected set; }

        [JsonProperty] public LifecycleExpiry Expiry { get; protected set; } = LifecycleExpiry.Pending;

        public void SetExpired(PointInTime currentTime)
        {
            ExpiredTime ??= new PointInTime(currentTime.Type, currentTime.Tick - 1);
        }

        public void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        { }

        public virtual void OnTimeBegins(RpgGraph graph, RpgObject? entity = null)
        { }

        public virtual LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time)
        {
            StartTime = TimePoints.BeginningOfTime;
            EndTime = TimePoints.EndOfTime;

            Expiry = graph.Time.CalculateExpiry(StartTime, EndTime);
            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint time)
        {
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);
            return Expiry;
        }
    }
}
