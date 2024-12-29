using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.States
{
    public abstract class State : ModSet
    {
        [JsonProperty] public string? OwnerArchetype { get; protected set; }
        [JsonProperty] public bool IsPlayerVisible { get; protected set; } = true;
        [JsonProperty] public List<Lifespan> TimedActivations { get; protected set; } = new();

        [JsonConstructor] protected State() { }

        protected State(string ownerId, string ownerArchetype)
            : base(ownerId, false)
        {
            Name = this.GetType().Name;
            OwnerArchetype = ownerArchetype;
        }

        public string Activate(string ownerId, TimePoint start, TimePoint end)
        {
            var lifespan = TimedActivations.FirstOrDefault(x => x.OwnerId == ownerId && x.Start == start && x.End == end);
            if (lifespan == null)
            {
                lifespan = new Lifespan(ownerId, start, end);
                TimedActivations.Add(lifespan);
            }

            return lifespan.Id;
        }

        public void Deactivate(string lifespanId)
        {
            var lifespan = TimedActivations.FirstOrDefault(x => x.Id == lifespanId);
            if (lifespan != null)
                TimedActivations.Remove(lifespan);
        }

        public void Deactivate(string ownerId, TimePoint start, TimePoint end)
        {
            var lifespan = TimedActivations.FirstOrDefault(x => x.OwnerId == ownerId && x.Start == start && x.End == end);
            if (lifespan != null)
                TimedActivations.Remove(lifespan);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            foreach (var activation in TimedActivations)
                activation.OnTimeEvent(graph);
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
            if (!IsApplied)
                return LifecycleExpiry.Suspended;

            if (!IsDisabled)
                return LifecycleExpiry.Active;

            var owner = graph.RefreshObject(OwnerId!) as T;
            if (owner != null)
            {
                if (IsOnWhen(owner))
                    return LifecycleExpiry.Active;
            }

            if (TimedActivations.Any(x => x.Expiry == LifecycleExpiry.Active))
                return LifecycleExpiry.Active;

            return LifecycleExpiry.Suspended;
        }
    }
}
