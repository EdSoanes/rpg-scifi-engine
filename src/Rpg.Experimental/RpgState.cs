using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public abstract class RpgState : RpgModSet
    {
        internal static string StatePropName(string stateName)
            => $"State/{stateName}";

        internal static bool IsStateProp(string prop)
            => prop.StartsWith("State/");

        [JsonProperty] public string? OwnerArchetype { get; protected set; }
        [JsonProperty] public bool IsPlayerVisible { get; protected set; } = true;
        [JsonProperty] public string Classification { get; protected set; } = "State";

        [JsonProperty] public bool IsOn { get; private set; }

        [JsonConstructor] protected RpgState() { }

        protected RpgState(string ownerId, string ownerArchetype)
            : base(ownerId, false)
        {
            Name = GetType().Name;
            OwnerArchetype = ownerArchetype;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            SyncOwnerActiveStates(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);
            SyncOwnerActiveStates(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            SyncOwnerActiveStates(graph);
        }

        private void SyncOwnerActiveStates(RpgGraph graph)
        {
            IsOn = Expiry == LifecycleExpiry.Active;
            graph.GetObject(OwnerId)?.SetStateOn(Name ?? GetType().Name, IsOn);
        }
    }

    public abstract class RpgState<T> : RpgState
        where T : RpgObject
    {
        [JsonConstructor] protected RpgState() { }

        public RpgState(T owner)
            : base(owner.Id, owner.Archetype)
        {
        }

        protected virtual bool IsOnWhen(T owner)
            => false;

        protected override LifecycleExpiry CalculateExpiry(RpgGraph graph, TimePoint start, TimePoint end)
        {
            var owner = graph.GetObject(OwnerId) as T;
            if (owner == null)
                return LifecycleExpiry.Destroyed;

            graph.OnSyncProperties(OwnerId!);

            if (owner.Expiry != LifecycleExpiry.Active)
                return owner.Expiry;

            if (!IsApplied)
                return LifecycleExpiry.Suspended;

            if (IsUserEnabled != null)
            {
                if (IsUserEnabled == true)
                    return LifecycleExpiry.Active;
                else
                    return LifecycleExpiry.Suspended;
            }

            if (IsOnWhen(owner))
                return LifecycleExpiry.Active;

            var activations = graph.GetPropertyValue<int>(owner, StatePropName(Name ?? GetType().Name));
            if (activations > 0)
                return LifecycleExpiry.Active;

            //if (TimedActivations.Any(x => x.Expiry == LifecycleExpiry.Active))
            //    return LifecycleExpiry.Active;


            return LifecycleExpiry.Suspended;
        }
    }
}
