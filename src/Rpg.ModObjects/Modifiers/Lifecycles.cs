using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Modifiers
{
    public abstract class BaseLifecycle : ITimeLifecycle
    {
        [JsonProperty] public Time.Time? ExpiredTime { get; protected set; }
        [JsonProperty] public Time.Time StartTime { get; protected set; }
        [JsonProperty] public Time.Time EndTime { get; protected set; }

        public ModExpiry Expiry { get; protected set; }

        public void SetExpired(Time.Time currentTime)
        {
            if (Expiry == ModExpiry.Active)
                ExpiredTime = new Time.Time(currentTime.Type, currentTime.Tick - 1);
        }

        public virtual ModExpiry StartLifecycle(RpgGraph graph, Time.Time currentTime, Mod? mod = null)
        {
            StartTime = graph.Time.BeginningOfTime;
            EndTime = graph.Time.EndOfTime;

            Expiry = graph.Time.CalculateExpiry(StartTime, EndTime);
            return Expiry;
        }

        public virtual ModExpiry UpdateLifecycle(RpgGraph graph, Time.Time currentTime, Mod? mod = null)
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
        public override ModExpiry StartLifecycle(RpgGraph graph, Time.Time currentTime, Mod? mod = null)
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

        public override ModExpiry UpdateLifecycle(RpgGraph graph, Time.Time currentTime, Mod? mod = null)
        {
            var modSet = graph.GetModSet(mod?.SyncedToId);

            Expiry = modSet?.Lifecycle.Expiry ?? ModExpiry.Remove;
            return Expiry;
        }
    }

    public class TimeLifecycle : BaseLifecycle
    {
        [JsonProperty] public Time.Time? Delay { get; private set; }
        [JsonProperty] public Time.Time? Duration { get; private set; }

        public TimeLifecycle(Time.Time duration)
        {
            Duration = duration;
        }

        public TimeLifecycle(Time.Time delay, Time.Time duration)
        {
            Delay = delay;
            Duration = duration;
        }

        public override ModExpiry StartLifecycle(RpgGraph graph, Time.Time time, Mod? mod = null)
        {
            StartTime = Delay == null
                ? graph.Time.BeginningOfTime
                : graph.Time.CalculateStartTime(Delay);

            EndTime = Duration == null
                ? graph.Time.EndOfTime
                : graph.Time.CalculateEndTime(StartTime, Duration);

            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);

            return Expiry;
        }
    }
}
