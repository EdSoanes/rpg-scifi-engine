using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Refs
{
    public abstract class RpgRef<T> : RpgLifecycleObject
        where T : class
    {
        protected class RpgObjRef<T>
            where T : class
        {
            [JsonProperty] public SpanOfTime Lifespan { get; set; }
            [JsonProperty] internal T Obj { get; set; }

            [JsonConstructor] RpgObjRef() { }

            public RpgObjRef(T obj, SpanOfTime lifespan)
            {
                Obj = obj;
                Lifespan = lifespan;
            }
            public RpgObjRef(T obj, PointInTime start, PointInTime end)
                : this(obj, new SpanOfTime(start, end))
                    => Obj = obj;
        }

        [JsonProperty] private List<RpgObjRef<T>> Refs { get; set; } = new();

        public static implicit operator RpgRef<T>(T obj) => new RpgRef<T>(obj);

        public RpgRef() { }

        public RpgRef(T obj)
        {
            Set(obj);
        }

        public RpgRef(RpgGraph graph)
        {
            OnCreating(graph);
            OnTimeBegins();
        }

        public RpgRef(RpgGraph graph, T obj)
        {
            OnCreating(graph);
            OnTimeBegins();
            Set(obj);
        }

        protected T? Get()
            => Refs.LastOrDefault(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active)?.Obj;

        protected void Set(T? obj, SpanOfTime? spanOfTime = null)
        {
            if (Graph != null)
            {
                if (spanOfTime != null)
                    spanOfTime.SetStartTime(Graph.Time.Now);
                else
                    spanOfTime = new SpanOfTime(Graph.Time.Now, PointInTimeType.TimeEnds);

                (obj as SpanOfTime)?.SetStartTime(Graph.Time.Now);
            }

            foreach (var existing in Refs.Where(x => x.Lifespan.End.Type == PointInTimeType.TimeEnds))
                existing.Lifespan = new SpanOfTime(existing.Lifespan.Start, Graph.Time.Now);

            if (obj != null && Get() != obj)
                Refs.Add(new RpgObjRef<T>(obj, spanOfTime ?? new SpanOfTime()));
        }

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
            base.OnUpdateLifecycle();
            if (!Graph.Time.Now.IsEncounterTime)
            {
                Refs = Refs.Where(x =>
                {
                    var expiry = x.Lifespan.GetExpiry(Graph.Time.Now);
                    return expiry != LifecycleExpiry.Expired && expiry != LifecycleExpiry.Destroyed;
                })
                .ToList();
            }

            return Expiry;
        }
    }
}
