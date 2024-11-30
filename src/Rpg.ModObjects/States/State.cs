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
        [JsonProperty] protected List<StateActivation> ActivationScheme { get; set; } = new();

        public bool IsOn { get => GetInstances().Any(); }
        public bool IsOnTimed { get => GetInstances().Any(x => x.InstanceType == StateInstanceType.Timed); }
        public bool IsOnManually { get => GetInstances().Any(x => x.InstanceType == StateInstanceType.Manual); }
        public bool IsOnConditionally { get => GetInstances().Any(x => x.InstanceType == StateInstanceType.Conditional); }

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
            var instance = GetInstances().FirstOrDefault();
            if (instance == null)
            {
                instance = new StateModSet(OwnerId, Name, StateInstanceType.Manual);
                instance.OnCreating(Graph);
                FillStateSet(instance);
            }

            return instance.Describe();
        }

        public void Activate()
        {
            if (!ActivationScheme.Any(x => x.InstanceType == StateInstanceType.Manual))
                ActivationScheme.Add(new StateActivation(StateInstanceType.Manual));
        }

        public void Deactivate()
        {
            if (ActivationScheme.Any(x => x.InstanceType == StateInstanceType.Manual))
                ActivationScheme = ActivationScheme
                    .Where(x => x.InstanceType != StateInstanceType.Manual)
                    .ToList();
        }

        public void Activate(string initiatorId, Lifespan lifespan)
        {
            lifespan.SetStartTime(Graph.Time.Now);
            var activation = new StateActivation(initiatorId, lifespan);
            if (!ActivationScheme.Any(x => x.Matches(activation)))
            {
                activation.UpdateExpiry(Graph.Time.Now);
                ActivationScheme.Add(activation);
            }
        }

        public void Deactivate(string initiatorId, Lifespan? lifespan = null)
            => ActivationScheme = ActivationScheme
                .Where(x => !x.Matches(initiatorId, lifespan))
                .ToList();
        
        internal StateModSet[] GetInstances()
            => Graph?
                .GetModSets<StateModSet>(OwnerId, (x) => x.StateName == Name && x.Expiry == LifecycleExpiry.Active) ?? [];

        internal abstract void FillStateSet(StateModSet modSet);
        protected abstract LifecycleExpiry CalculateExpiry();


        public void OnCreating(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnRestoring(RpgGraph graph, RpgObject? entity = null)
            => Graph = graph;

        public void OnTimeBegins() { }

        public LifecycleExpiry OnStartLifecycle()
        {
            foreach (var item in ActivationScheme)
                item.UpdateExpiry(Graph.Time.Now);

            Expiry = CalculateExpiry();
            UpdateInstanceActivation();

            return Expiry;
        }

        public LifecycleExpiry OnUpdateLifecycle()
        {
            foreach (var item in ActivationScheme)
                item.UpdateExpiry(Graph.Time.Now);

            Expiry = CalculateExpiry();
            UpdateInstanceActivation();

            return Expiry;
        }

        protected StateActivation? CurrentActivation(StateInstanceType? instanceType = null)
            => ActivationScheme
                .Where(x => x.Expiry == LifecycleExpiry.Active && (instanceType == null || x.InstanceType == instanceType))
                .OrderBy(x => x.InstanceType)
                .FirstOrDefault();

        private void UpdateInstanceActivation()
        {
            var stateInstance = Graph.GetModSets<StateModSet>(OwnerId, (x) => x.StateName == Name).FirstOrDefault();
            var activation = CurrentActivation();

            if (activation != null)
            {
                if (stateInstance == null)
                {
                    stateInstance = new StateModSet(OwnerId, Name, activation.InstanceType);
                    FillStateSet(stateInstance);
                    Graph.AddModSets(stateInstance);
                }
                else if (stateInstance.InstanceType != activation.InstanceType)
                    stateInstance.InstanceType = activation.InstanceType;
            }

            else if (stateInstance != null && activation != null)
            {
                stateInstance.InstanceType = activation.InstanceType;
            }
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
            var isConditionallyOn = IsOnWhen(owner);
            var conditionalActivation = CurrentActivation(StateInstanceType.Conditional);
            if (conditionalActivation == null && isConditionallyOn)
            {
                var newActivation = new StateActivation(StateInstanceType.Conditional);
                newActivation.UpdateExpiry(Graph.Time.Now);
                ActivationScheme.Add(newActivation);
            }
            else if (!isConditionallyOn)
            {
                ActivationScheme = ActivationScheme
                    .Where(x => x.InstanceType != StateInstanceType.Conditional)
                    .ToList();
            }

            var activation = CurrentActivation();
            var expiry = activation != null
                ? LifecycleExpiry.Active
                : LifecycleExpiry.Expired;

            if (expiry == LifecycleExpiry.Expired && !Graph.Time.Now.IsEncounterTime)
                expiry = LifecycleExpiry.Destroyed;

            return expiry;
        }
    }
}
