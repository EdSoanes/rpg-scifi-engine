using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.States
{
    public abstract class State
    {
        protected RpgGraph Graph { get; set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] public ILifecycle Lifecycle { get; set; }

        public bool IsOn { get => IsOnConditionally || IsOnManually; }
        public bool IsOnConditionally { get => Graph?.GetEntity<RpgEntity>(OwnerId)!.GetActiveConditionalStateInstances(Name).Any() ?? false; }
        public bool IsOnManually { get => Graph?.GetEntity<RpgEntity>(OwnerId)!.GetActiveManualStateInstances(Name).Any() ?? false; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
        {
            Id = this.NewId();
            Name = GetType().Name;
            OwnerId = owner.Id;
            OwnerArchetype = owner.Archetype;
        }

        public ModSet CreateInstance(ILifecycle? lifecycle = null)
        {
            var modSet = new ModSet(OwnerId!, lifecycle ?? new SyncedLifecycle(Id), Name);
            FillStateSet(modSet);

            return modSet;
        }

        internal abstract void FillStateSet(ModSet modSet);

        internal void OnAdding(RpgGraph graph)
        {
            Graph = graph;
        }

        public bool On()
            => On(new PermanentLifecycle());

        public bool On(ILifecycle lifecycle)
        {
            if (Graph == null)
                return false;
            
            if (!IsOnManually)
            {
                var stateSet = CreateInstance(new PermanentLifecycle());
                Graph.AddModSets(stateSet);
            }

            return true;
        }

        public bool Off()
        {
            if (Graph == null)
                return false;

            if (IsOnManually)
            {
                var entity = Graph.GetEntity<RpgEntity>(OwnerId)!;
                var stateSets = entity.GetActiveManualStateInstances(Name);

                foreach (var stateSet in stateSets)
                    entity.ModSetStore.Remove(stateSet.Id);
            }

            return true;
        }
    }

    public abstract class State<T> : State
        where T : RpgEntity
    {
        [JsonConstructor] protected State() { }

        public State(T owner)
            : base(owner)
        {
            Lifecycle = new ConditionalLifecycle<State<T>>(Id, new RpgMethod<State<T>, LifecycleExpiry>(this, nameof(CalculateExpiry)));
        }

        protected virtual bool IsOnWhen(T owner)
            => false;

        internal override void FillStateSet(ModSet modSet)
        {
            var owner = Graph!.GetEntity<T>(OwnerId)!;
            OnFillStateSet(modSet, owner);
        }

        protected virtual void OnFillStateSet(ModSet modSet, T owner)
        { }

        protected virtual LifecycleExpiry CalculateExpiry()
        {
            var entity = Graph!.GetEntity<T>(OwnerId)!;
            return IsOnWhen(entity)
                ? LifecycleExpiry.Active
                : LifecycleExpiry.Expired;
        }
    }
}
