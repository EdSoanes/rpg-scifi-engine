using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;
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
            StartTime = TimePoints.BeginningOfTime;
            EndTime = TimePoints.EndOfTime;

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

    public class ConditionalLifecycle<TOwner> : BaseLifecycle
        where TOwner : class
    {
        [JsonProperty] public string OwnerId { get; private set; }
        [JsonProperty] public RpgMethod<TOwner, LifecycleExpiry> OnStartConditional { get; private set; }
        [JsonProperty] public RpgMethod<TOwner, LifecycleExpiry> OnUpdateConditional { get; private set; }

        public ConditionalLifecycle(
            string entityId,
            RpgMethod<TOwner, LifecycleExpiry> onStartConditional,
            RpgMethod<TOwner, LifecycleExpiry> onUpdateConditional)
        {
            OwnerId = entityId;
            OnStartConditional = onStartConditional;
            OnUpdateConditional = onUpdateConditional;
        }

        public ConditionalLifecycle(string entityId, RpgMethod<TOwner, LifecycleExpiry> conditional)
            : this(entityId, conditional, conditional)
        { }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var owner = GetOwner(graph);
            var argSet = OnStartConditional.Create();

            Expiry = OnStartConditional.Execute(owner, argSet);
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var owner = GetOwner(graph);
            var argSet = OnUpdateConditional.Create();

            Expiry = OnUpdateConditional.Execute(owner, argSet);
            return Expiry;
        }

        private TOwner GetOwner(RpgGraph graph)
        {
            var entity = graph.GetEntity(OwnerId)!;
            if (entity != null && entity.GetType() == typeof(TOwner))
                return (entity as TOwner)!;

            var state = graph.GetState(OwnerId);
            if (state != null && state.GetType() == typeof(TOwner))
                return (state as TOwner)!;

            var modSet = graph.GetModSet(OwnerId);
            if (modSet != null && modSet.GetType() == typeof(TOwner))
                return (modSet as TOwner)!;

            throw new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");
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

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time, Mod? mod = null)
        {
            StartTime = graph.Time.CalculateStartTime(Delay);
            EndTime = graph.Time.CalculateEndTime(StartTime, Duration);
            Expiry = graph.Time.CalculateExpiry(StartTime, ExpiredTime ?? EndTime);

            return Expiry;
        }
    }
}
