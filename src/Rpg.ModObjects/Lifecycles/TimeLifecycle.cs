using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public class TimeLifecycle : BaseLifecycle
    {
        [JsonProperty] public TimePoint Delay { get; private set; } = TimePoints.Empty;
        [JsonProperty] public TimePoint Duration { get; private set; } = TimePoints.EndOfTime;

        public TimeLifecycle(TimePoint duration)
        {
            Duration = duration;
        }

        public TimeLifecycle(TimePoint delay, TimePoint duration)
        {
            Delay = delay;
            Duration = duration;
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time)
        {
            StartTime = graph.Time.CalculateStartTime(Delay);
            EndTime = graph.Time.CalculateEndTime(StartTime, Duration);
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);

            return Expiry;
        }
    }
}
