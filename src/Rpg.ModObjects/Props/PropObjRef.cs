using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Props
{
    public class PropObjRef<T> : RpgLifecycleObject
        where T : class
    {
        protected class InternalRef<T>
            where T : class
        {
            [JsonProperty] public SpanOfTime Lifespan { get; set; }
            [JsonProperty] internal T Obj { get; set; }

            [JsonConstructor] InternalRef() { }

            public InternalRef(T obj, SpanOfTime lifespan)
            {
                Obj = obj;
                Lifespan = lifespan;
            }

            public InternalRef(T obj, PointInTime start, PointInTime end)
                : this(obj, new SpanOfTime(start, end))
                    => Obj = obj;
        }

        [JsonProperty] private List<InternalRef<T>> Refs { get; set; } = new();

        public static implicit operator PropObjRef<T>(T obj) => new PropObjRef<T>(obj);

        public PropObjRef() { }

        public PropObjRef(T obj)
        {
            Set(obj);
        }

        public PropObjRef(RpgGraph graph)
        {
            OnCreating(graph);
            OnTimeBegins();
        }

        public PropObjRef(RpgGraph graph, T obj)
        {
            OnCreating(graph);
            OnTimeBegins();
            
            Lifespan = GetLifespan();
            Set(obj, Lifespan);
        }

        public virtual T? Get()
            => Graph != null
                ? Refs.LastOrDefault(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active)?.Obj
                : Refs.LastOrDefault(x => x.Lifespan.GetExpiry(PointInTimeType.TimeBegins) == LifecycleExpiry.Active)?.Obj;

        private SpanOfTime GetLifespan(SpanOfTime? lifespan = null)
        {
            if (lifespan != null)
                lifespan.SetStartTime(Graph.Time.Now);
            else 
                lifespan = new SpanOfTime(Graph?.Time.Now ?? PointInTimeType.TimeBegins, PointInTimeType.TimeEnds, true);

            return lifespan;
        }

        public virtual void Set(T? obj, SpanOfTime? lifespan = null)
        {
            lifespan = GetLifespan(lifespan);
            if (Graph != null)
            {
                foreach (var intRef in Refs.Where(x => x.Lifespan.End.Type == PointInTimeType.TimeEnds))
                {
                    if (intRef.Obj == obj && intRef.Lifespan.End >= lifespan.Start)
                        intRef.Lifespan = new SpanOfTime(intRef.Lifespan.Start, lifespan.End);
                    else
                        intRef.Lifespan = new SpanOfTime(intRef.Lifespan.Start, Graph.Time.Now);
                }
            }

            if (obj != null && Get() != obj)
                Refs.Add(new InternalRef<T>(obj, lifespan));
        }

        public override void SetExpired()
            => Set(null);

        protected override void CalculateExpiry()
        {
            var idx = Refs.FindIndex(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active);
            if (idx >= 0)
            {
                Expiry = LifecycleExpiry.Active;
                return;
            }

            if (Refs.Count() == 0)
            {
                Expiry = LifecycleExpiry.Unset;
                return;
            }

            if (Refs.Any(x => x.Lifespan.Start > Graph.Time.Now))
            {
                Expiry = LifecycleExpiry.Pending;
                return;
            }

            Expiry = Graph.Time.Now.IsEncounterTime
                ? LifecycleExpiry.Expired
                : LifecycleExpiry.Destroyed;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var now = Graph.Time.Now;
            if (!now.IsEncounterTime)
                Refs = GetConsolidatedRefs();

            CalculateExpiry();
            return Expiry;
        }

        private List<InternalRef<T>> GetConsolidatedRefs()
        {
            var now = Graph.Time.Now;
            var updatedRefs = Refs.Where(x =>
            {
                //If the encounter ends and there are refs that should continue to live after the encounter, change the lifespan start to TimePassing
                // so they do not expire.
                if (now == PointInTimeType.EncounterEnds && x.Lifespan.Start.Type == PointInTimeType.Turn && x.Lifespan.End == PointInTimeType.TimeEnds)
                    x.Lifespan = Lifespan = new SpanOfTime(PointInTimeType.TimePassing, PointInTimeType.TimeEnds);

                var expiry = x.Lifespan.GetExpiry(Graph.Time.Now);
                return expiry != LifecycleExpiry.Expired && expiry != LifecycleExpiry.Destroyed;
            })
            .ToList();

            return updatedRefs;
        }

        public override string ToString()
        {
            return $"[{Lifespan}] {Get()}";
        }
    }
}
