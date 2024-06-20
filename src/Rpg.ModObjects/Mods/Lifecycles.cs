using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public abstract class BaseLifecycle : ILifecycle
    {
        [JsonProperty] public TimePoint? ExpiredTime { get; protected set; }
        [JsonProperty] public TimePoint StartTime { get; protected set; }
        [JsonProperty] public TimePoint EndTime { get; protected set; }

        public LifecycleExpiry Expiry { get; protected set; }

        public void SetExpired(TimePoint currentTime)
        {
            if (Expiry == LifecycleExpiry.Active)
                ExpiredTime = new TimePoint(currentTime.Type, currentTime.Tick - 1);
        }

        public virtual void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        { }

        public virtual LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            StartTime = graph.Time.BeginningOfTime;
            EndTime = graph.Time.EndOfTime;

            Expiry = graph.Time.CalculateExpiry(StartTime, EndTime);
            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);
            return Expiry;
        }
    }

    public class PermanentLifecycle : BaseLifecycle
    {
    }

    public class ConditionalLifecycle : BaseLifecycle
    {
        public Action<RpgGraph, RpgObject?>? OnBeginConditional { get; private set; }
        public Func<LifecycleExpiry> OnStartConditional { get; private set; }
        public Func<LifecycleExpiry> OnUpdateConditional { get; private set; }

        public ConditionalLifecycle(
            Action<RpgGraph, RpgObject?>? onBeginConditional, 
            Func<LifecycleExpiry> onStartConditional, 
            Func<LifecycleExpiry> onUpdateConditional)
        {
            OnBeginConditional = onBeginConditional;
            OnStartConditional = onStartConditional;
            OnUpdateConditional = onUpdateConditional;
        }

        public ConditionalLifecycle(Func<LifecycleExpiry> conditional)
            : this(null, conditional, conditional)
        { }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            if (OnBeginConditional != null)
                OnBeginConditional.Invoke(graph, entity);
            else 
                base.OnBeginningOfTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            Expiry = OnStartConditional.Invoke();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            Expiry = OnStartConditional.Invoke();
            return Expiry;
        }
    }

    public class SyncedLifecycle : BaseLifecycle
    {
        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            if (!string.IsNullOrEmpty(mod?.SyncedToId))
            {
                if (mod.SyncedToType == nameof(ModSet))
                {
                    var syncedToModSet = graph.GetModSet(mod?.SyncedToId);
                    Expiry = syncedToModSet?.Lifecycle.Expiry ?? LifecycleExpiry.Remove;
                }
                else if (mod.SyncedToType == nameof(Mod))
                {
                    var syncedToMod = graph.GetMods().FirstOrDefault(x => x.Id == mod?.SyncedToId);
                    Expiry = syncedToMod?.Lifecycle.Expiry ?? LifecycleExpiry.Remove;
                }
            }

            
            return base.OnStartLifecycle(graph, currentTime, mod);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var modSet = graph.GetModSet(mod?.SyncedToId);

            Expiry = modSet?.Lifecycle.Expiry ?? LifecycleExpiry.Remove;
            return Expiry;
        }
    }

    public class TimeLifecycle : BaseLifecycle
    {
        [JsonProperty] public TimePoint? Delay { get; private set; }
        [JsonProperty] public TimePoint? Duration { get; private set; }

        public TimeLifecycle(TimePoint duration)
        {
            Duration = duration;
        }

        public TimeLifecycle(TimePoint delay, TimePoint duration)
        {
            Delay = delay;
            Duration = duration;
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time, Mod? mod = null)
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
