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
        [JsonProperty] public string OwnerId { get; private init; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] public ILifecycle Lifecycle { get; set; }

        public bool IsOn { get => IsOnConditionally || IsOnManually; }
        public bool IsOnConditionally { get => Graph?.GetObject(OwnerId)!.GetActiveConditionalStateInstances(Name).Any() ?? false; }
        public bool IsOnManually { get => Graph?.GetObject(OwnerId)!.GetActiveManualStateInstances(Name).Any() ?? false; }

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

        internal virtual void OnAdding(RpgGraph graph)
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
                var entity = Graph.GetObject(OwnerId)!;
                var stateSets = entity.GetActiveManualStateInstances(Name);

                foreach (var stateSet in stateSets)
                    entity.RemoveModSet(stateSet.Id);
            }

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

        internal override void OnAdding(RpgGraph graph)
        {
            base.OnAdding(graph);

            var conditionalMethod = RpgMethod.Create<State<T>, LifecycleExpiry>(this, nameof(CalculateExpiry))!;
            Lifecycle = new ConditionalLifecycle<State<T>>(Id, conditionalMethod);
            Lifecycle.OnBeforeTime(graph);
            Lifecycle.OnTimeBegins();
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

        protected virtual LifecycleExpiry CalculateExpiry()
        {
            var entity = Graph!.GetObject<T>(OwnerId)!;
            return IsOnWhen(entity)
                ? LifecycleExpiry.Active
                : LifecycleExpiry.Expired;
        }
    }
}
