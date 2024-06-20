using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public abstract class BaseLifecycle : ILifecycle
    {
        [JsonProperty] public TimePoint? ExpiredTime { get; protected set; }
        [JsonProperty] public TimePoint StartTime { get; protected set; }
        [JsonProperty] public TimePoint EndTime { get; protected set; }

        public ModExpiry Expiry { get; protected set; }

        public void SetExpired(TimePoint currentTime)
        {
            if (Expiry == ModExpiry.Active)
                ExpiredTime = new TimePoint(currentTime.Type, currentTime.Tick - 1);
        }

        public virtual ModExpiry StartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            StartTime = TimePoints.BeginningOfTime;
            EndTime = TimePoints.EndOfTime;

            Expiry = graph.Time.CalculateExpiry(StartTime, EndTime);
            return Expiry;
        }

        public virtual ModExpiry UpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);
            return Expiry;
        }
    }

    public class PermanentLifecycle : BaseLifecycle
    {
    }

    public class SyncedLifecycle : BaseLifecycle
    {
        public override ModExpiry StartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            if (!string.IsNullOrEmpty(mod?.SyncedToId))
            {
                if (mod.SyncedToType == nameof(ModSet))
                {
                    var syncedToModSet = graph.GetModSet(mod?.SyncedToId);
                    Expiry = syncedToModSet?.Lifecycle.Expiry ?? ModExpiry.Remove;
                }
                else if (mod.SyncedToType == nameof(Mod))
                {
                    var syncedToMod = graph.GetMods().FirstOrDefault(x => x.Id == mod?.SyncedToId);
                    Expiry = syncedToMod?.Lifecycle.Expiry ?? ModExpiry.Remove;
                }
            }

            
            return base.StartLifecycle(graph, currentTime, mod);
        }

        public override ModExpiry UpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var modSet = graph.GetModSet(mod?.SyncedToId);

            Expiry = modSet?.Lifecycle.Expiry ?? ModExpiry.Remove;
            return Expiry;
        }
    }

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

        public override ModExpiry StartLifecycle(RpgGraph graph, TimePoint time, Mod? mod = null)
        {
            StartTime = graph.Time.CalculateStartTime(Delay);
            EndTime = graph.Time.CalculateEndTime(StartTime, Duration);
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);

            return Expiry;
        }
    }
}
