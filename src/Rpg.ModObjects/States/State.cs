using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    public abstract class State : RpgLifecycleObject
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string OwnerId { get; private init; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }
        [JsonProperty] protected SpanOfTime? ManualLifespan { get; set; }

        public bool IsOn { get => IsOnConditionally || IsOnManually; }
        public bool IsOnConditionally { get => Graph?.GetObject(OwnerId)!.GetActiveConditionalStateInstances(Name).Any() ?? false; }
        public bool IsOnManually { get => ManualLifespan != null; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
        {
            Id = this.NewId();
            Name = this.GetType().Name;
            OwnerId = owner.Id;
            OwnerArchetype = owner.Archetype;
        }

        public ModSet CreateInstance(SpanOfTime? spanOfTime = null)
        {
            ModSet modSet = spanOfTime != null
                ? new TimedModSet(OwnerId, Name, spanOfTime!)
                : new SyncedModSet(OwnerId!, Id, Name);

            FillStateSet(modSet);

            return modSet;
        }

        internal abstract void FillStateSet(ModSet modSet);

        public bool On()
        {
            ManualLifespan ??= new SpanOfTime();
            return true;
        }

        public bool Off()
        {
            ManualLifespan = null;
            return true;
        }

        public static State[] CreateOwnerStates(RpgObject entity)
        {
            var states = new List<State>();

            var types = RpgTypeScan.ForTypes<State>()
                .Where(x => IsOwnerStateType(entity, x));

            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(state.OwnerArchetype!))
                    states.Add(state);
            }

            return states.ToArray();
        }

        private static bool IsOwnerStateType(RpgObject entity, Type? stateType)
        {
            while (stateType != null)
            {
                if (stateType.IsGenericType)
                {
                    var genericTypes = stateType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                stateType = stateType.BaseType;
            }

            return false;
        }
    }

    public abstract class State<T> : State
        where T : RpgObject
    {
        [JsonConstructor] protected State() { }

        public State(T owner)
            : base(owner)
        {
        }

        protected virtual bool IsOnWhen(T owner)
            => false;

        internal override void FillStateSet(ModSet modSet)
        {
            var owner = Graph!.GetObject<T>(OwnerId)!;
            OnFillStateSet(modSet, owner);
        }

        protected virtual void OnFillStateSet(ModSet modSet, T owner)
        { }

        public override LifecycleExpiry OnStartLifecycle()
        {
            Lifespan.SetStartTime(Graph.Time.Current);
            return CalculateExpiry();
        }

        public override LifecycleExpiry OnUpdateLifecycle()
            => CalculateExpiry();

        protected virtual LifecycleExpiry CalculateExpiry()
        {
            var expiry = ManualLifespan?.GetExpiry(Graph.Time.Current) ?? LifecycleExpiry.Unset;
            if (expiry == LifecycleExpiry.Unset)
            {
                var owner = Graph!.GetObject<T>(OwnerId)!;
                expiry = IsOnWhen(owner)
                    ? LifecycleExpiry.Active
                    : LifecycleExpiry.Expired;
            }

            Expiry = expiry != LifecycleExpiry.Unset ? expiry : LifecycleExpiry.Expired;
            return Expiry;
        }
    }
}
