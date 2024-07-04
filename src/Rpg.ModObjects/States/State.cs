using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.States
{
    public abstract class State
    {
        protected RpgGraph Graph { get; set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        public ILifecycle Lifecycle { get => ForcedLifecycle ?? ConditionalLifecycle; }
        [JsonProperty] protected ILifecycle ConditionalLifecycle { get; set; }
        [JsonProperty] protected ILifecycle? ForcedLifecycle { get; set; }

        [JsonProperty] public bool ConditionallyOn { get; protected set; }
        public bool IsOn { get => Lifecycle.Expiry == LifecycleExpiry.Active; }

        [JsonConstructor] protected State() { }

        protected State(RpgObject owner)
        {
            Id = this.NewId();
            Name = GetType().Name;
            OwnerId = owner.Id;
            OwnerArchetype = owner.Archetype;
        }

        internal abstract void FillStateSet(ModSet modSet);

        internal void OnAdding(RpgGraph graph)
        {
            Graph = graph;
        }

        public bool On()
        {
            if (ForcedLifecycle == null)
            {
                ForcedLifecycle = new PermanentLifecycle();
                ForcedLifecycle.OnStartLifecycle(Graph!, Graph!.Time.Current);
            }

            return true;
        }

        public bool On(ILifecycle lifecycle)
        {
            ForcedLifecycle = lifecycle;
            return true;
        }

        public bool Off()
        {
            ForcedLifecycle = null;
            return true;
        }
    }

    public abstract class State<T> : State
        where T : RpgObject
    {
        [JsonConstructor] protected State() { }

        public State(T owner)
            : base(owner)
        {
            ConditionalLifecycle = new ConditionalLifecycle<State<T>>(Id, new RpgMethod<State<T>, LifecycleExpiry>(this, nameof(CalculateExpiry)));
        }

        protected virtual bool IsOnWhen(T owner)
            => false;

        public ModSet CreateStateInstance(string stateName, ILifecycle? lifecycle = null)
        {
            var owner = Graph!.GetEntity<T>(Id)!;
            var modSet = new ModSet(owner, lifecycle ?? new SyncedLifecycle(Id), stateName);
            OnFillStateSet(modSet, owner);

            return modSet;
        }

        internal override void FillStateSet(ModSet modSet)
        {
            var owner = Graph!.GetEntity<T>(OwnerId)!;
            OnFillStateSet(modSet, owner);
        }

        protected virtual void OnFillStateSet(ModSet modSet, T owner)
        { }

        protected virtual LifecycleExpiry CalculateExpiry()
        {
            LifecycleExpiry expiry;
            if (ForcedLifecycle != null)
            {
                expiry = ForcedLifecycle.OnUpdateLifecycle(Graph!, Graph!.Time.Current);
            }
            else
            {
                var obj = Graph!.GetEntity<T>(OwnerId)!;
                expiry = IsOnWhen(obj)
                    ? LifecycleExpiry.Active
                    : LifecycleExpiry.Expired;

                //Should we do the following here? Won't the engine ensure that the mods are added later on?
                if (expiry == LifecycleExpiry.Active && !ConditionallyOn)
                {
                    ConditionallyOn = true;
                }
                else if (expiry != LifecycleExpiry.Active && ConditionallyOn)
                {
                    ConditionallyOn = false;
                }
            }

            return expiry;
        }
    }
}
