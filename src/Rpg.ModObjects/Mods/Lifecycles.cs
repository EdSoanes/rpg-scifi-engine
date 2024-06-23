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

        public LifecycleExpiry Expiry { get; protected set; } = LifecycleExpiry.Pending;

        public void SetExpired(TimePoint currentTime)
        {
            ExpiredTime ??= new TimePoint(currentTime.Type, currentTime.Tick - 1);
        }

        public void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        { }

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

        [JsonConstructor]private ConditionalLifecycle() { }

        public ConditionalLifecycle(
            string ownerId,
            RpgMethod<TOwner, LifecycleExpiry> onStartConditional,
            RpgMethod<TOwner, LifecycleExpiry> onUpdateConditional)
        {
            OwnerId = ownerId;
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
            var owner = graph.Locate<TOwner>(OwnerId);
            if (owner == null)
                new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");

            var argSet = OnStartConditional.Create();

            Expiry = OnStartConditional.Execute(owner!, argSet);
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var owner = graph.Locate<TOwner>(OwnerId);
            if (owner == null)
                new InvalidOperationException($"{nameof(ConditionalLifecycle<TOwner>)}.{nameof(GetOwner)} could not find owner");

            var argSet = OnUpdateConditional.Create();

            Expiry = OnUpdateConditional.Execute(owner!, argSet);
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
            var modSet = graph.Locate<ModSet>(mod?.OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.Expiry;
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(mod?.OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.Expiry;
                return Expiry;
            }

            return base.OnStartLifecycle(graph, currentTime, mod);
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
        {
            var modSet = graph.Locate<ModSet>(mod?.OwnerId);
            if (modSet != null)
            {
                Expiry = modSet.Lifecycle.Expiry;
                return Expiry;
            }

            var ownerMod = graph.Locate<Mod>(mod?.OwnerId);
            if (ownerMod != null)
            {
                Expiry = ownerMod.Lifecycle.Expiry;
                return Expiry;
            }

            return base.OnUpdateLifecycle(graph, currentTime, mod);
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
