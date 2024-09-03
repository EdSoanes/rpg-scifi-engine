using Newtonsoft.Json;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    public abstract class State : ILifecycle
    {
        protected RpgGraph Graph {  get; private set; }

        public LifecycleExpiry Expiry { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string OwnerId { get; private init; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }
        [JsonProperty] protected List<SpanOfTime> ActiveTimeSpans { get; set; } = new();

        public bool IsOn { get => OnByTimePeriod || OnByUserAction || OnByCondition; }
        public bool OnByTimePeriod { get => ActiveTimeSpans.Any(x => x.GetExpiry(Graph?.Time.Now ?? PointInTimeType.BeforeTime) == LifecycleExpiry.Active); }
        public bool OnByUserAction { get; private set; }
        public bool OnByCondition { get; protected set; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
        {
            Id = this.NewId();
            Name = this.GetType().Name;
            OwnerId = owner.Id;
            OwnerArchetype = owner.Archetype;
        }

        public StateModSet CreateInstance(SpanOfTime? spanOfTime = null)
        {
            if (spanOfTime != null && !ActiveTimeSpans.Any(x => x == spanOfTime))
                ActiveTimeSpans.Add(spanOfTime);

            var instance = GetInstance();
            if (instance == null)
            {
                instance = new StateModSet(OwnerId, Name);
                instance.Update(OnByUserAction, CalculateExpiry());
                FillStateSet(instance);
            }

            return instance;
        }

        public StateModSet? GetInstance()
            => Graph?.GetModSets<StateModSet>(OwnerId, (x) => x.StateName == Name && x.Expiry == LifecycleExpiry.Active).FirstOrDefault();

        internal abstract void FillStateSet(StateModSet modSet);
        protected abstract LifecycleExpiry CalculateExpiry();

        public void On()
            => OnByUserAction = true;

        public void Off()
            => OnByUserAction = false;

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

        public void OnCreating(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnRestoring(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnTimeBegins() { }

        public LifecycleExpiry OnStartLifecycle()
        {
            var expiry = CalculateExpiry();

            if (expiry == LifecycleExpiry.Active)
            {
                var instance = CreateInstance();
                Graph.AddModSets(instance);
            }

            Expiry = expiry;
            return expiry;
        }

        public LifecycleExpiry OnUpdateLifecycle()
        {
            ActiveTimeSpans = ActiveTimeSpans
                .Where(x => x.GetExpiry(Graph.Time.Now) != LifecycleExpiry.Destroyed)
                .ToList();

            var expiry = CalculateExpiry();

            var instance = GetInstance();
            if (instance != null)
                instance.Update(OnByUserAction, expiry);

            else if (expiry == LifecycleExpiry.Active)
            {
                instance = CreateInstance();
                Graph.AddModSets(instance);
            }

            Expiry = expiry;
            return expiry;
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

        internal override void FillStateSet(StateModSet modSet)
        {
            var owner = Graph!.GetObject<T>(OwnerId)!;
            OnFillStateSet(modSet, owner);
        }

        protected virtual void OnFillStateSet(StateModSet modSet, T owner)
        { }

        protected override LifecycleExpiry CalculateExpiry()
        {
            var expiry = OnByUserAction || OnByTimePeriod
                ? LifecycleExpiry.Active
                : LifecycleExpiry.Expired;

            if (expiry != LifecycleExpiry.Active)
            {
                var owner = Graph!.GetObject<T>(OwnerId)!;
                OnByCondition = IsOnWhen(owner);
                if (OnByCondition)
                    expiry = LifecycleExpiry.Active;
            }

            if (expiry == LifecycleExpiry.Expired && !Graph.Time.Now.IsEncounterTime)
                expiry = LifecycleExpiry.Destroyed;

            return expiry;
        }
    }
}
