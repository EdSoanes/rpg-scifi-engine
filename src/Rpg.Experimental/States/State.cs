using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.States
{
    public abstract class State : ModSet
    {
        internal static string StatePropName(string stateName)
            => $"State/{stateName}";

        [JsonProperty] public string? OwnerArchetype { get; protected set; }
        [JsonProperty] public bool IsPlayerVisible { get; protected set; } = true;

        [JsonConstructor] protected State() { }

        protected State(string ownerId, string ownerArchetype)
            : base(ownerId, false)
        {
            Name = this.GetType().Name;
            OwnerArchetype = ownerArchetype;
        }
    }

    public abstract class State<T> : State
        where T : RpgObject
    {
        [JsonConstructor] protected State() { }

        public State(T owner)
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
