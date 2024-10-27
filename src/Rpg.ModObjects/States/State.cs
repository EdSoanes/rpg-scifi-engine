using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Newtonsoft.Json;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.States
{
    public abstract class State : ILifecycle
    {
        protected RpgGraph Graph {  get; private set; }

        public LifecycleExpiry Expiry { get; protected set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string OwnerId { get; protected set; }
        [JsonProperty] public string? OwnerArchetype { get; protected set; }
        [JsonProperty] public bool IsPlayerVisible { get; protected set; } = true;
        [JsonProperty] protected List<SpanOfTime> ActiveTimeSpans { get; set; } = new();

        public bool IsOn { get => OnByTimePeriod || OnByUserAction || OnByCondition; }
        public bool OnByTimePeriod { get => ActiveTimeSpans.Any(x => x.GetExpiry(Graph?.Time.Now ?? PointInTimeType.BeforeTime) == LifecycleExpiry.Active); }
        public bool OnByUserAction { get; protected set; }
        public bool OnByCondition { get; protected set; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
        {
            Id = this.NewId();
            Name = this.GetType().Name;
            OwnerId = owner.Id;
            OwnerArchetype = owner.Archetype;
        }

        public ModSetDescription Describe()
        {
            var instance = new StateModSet(OwnerId, Name);
            instance.OnCreating(Graph);
            FillStateSet(instance);
            return instance.Describe();
        }

        public StateModSet? ActivateInstance(SpanOfTime? spanOfTime = null)
        {
            if (Graph != null)
            {
                if (spanOfTime != null)
                {
                    spanOfTime.SetStartTime(Graph.Time.Now);
                    if (!ActiveTimeSpans.Any(x => x == spanOfTime))
                        ActiveTimeSpans.Add(spanOfTime);
                }

                var instance = Graph?
                    .GetModSets<StateModSet>(OwnerId, (x) => x.StateName == Name && x.Expiry == LifecycleExpiry.Active)
                    .FirstOrDefault();

                if (instance == null)
                {
                    instance = new StateModSet(OwnerId, Name);
                    FillStateSet(instance);
                    Graph.AddModSets(instance);
                }

                return instance;
            }

            return null;
        }

        internal abstract void FillStateSet(StateModSet modSet);
        protected abstract LifecycleExpiry CalculateExpiry();

        public void On()
            => OnByUserAction = true;

        public void Off()
            => OnByUserAction = false;

        public void OnCreating(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnRestoring(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnTimeBegins() { }

        public LifecycleExpiry OnStartLifecycle()
        {
            Expiry = CalculateExpiry();
            if (Expiry == LifecycleExpiry.Active)
                ActivateInstance();

            return Expiry;
        }

        public LifecycleExpiry OnUpdateLifecycle()
        {
            ActiveTimeSpans = ActiveTimeSpans
                .Where(x => x.GetExpiry(Graph.Time.Now) != LifecycleExpiry.Destroyed)
                .ToList();

            Expiry = CalculateExpiry();
            if (Expiry == LifecycleExpiry.Active)
                ActivateInstance();

            return Expiry;
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
            var owner = Graph!.GetObject<T>(OwnerId)!;
            OnByCondition = IsOnWhen(owner);

            var expiry = OnByUserAction || OnByTimePeriod || OnByCondition
                ? LifecycleExpiry.Active
                : LifecycleExpiry.Expired;

            if (expiry == LifecycleExpiry.Expired && !Graph.Time.Now.IsEncounterTime)
                expiry = LifecycleExpiry.Destroyed;

            return expiry;
        }
    }
}
