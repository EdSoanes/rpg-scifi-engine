using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Props
{
    public class PropObjRef : RpgLifecycleObject
    {
        [JsonProperty] public string EntityId { get; protected set; }
        [JsonProperty] public string? OwnerId { get; protected set; }

        public static bool operator ==(PropObjRef? d1, PropObjRef? d2) => d1?.OwnerId == d2?.OwnerId && d1?.EntityId == d2?.EntityId && d1?.Lifespan == d2?.Lifespan;
        public static bool operator !=(PropObjRef? d1, PropObjRef? d2) => d1?.OwnerId != d2?.OwnerId || d1?.EntityId != d2?.EntityId || d1?.Lifespan != d2?.Lifespan;

        public PropObjRef(string entityId, SpanOfTime lifespan, string? ownerId = null)
        {
            EntityId = entityId;
            Lifespan = lifespan;
            OwnerId = ownerId;
        }

        public override bool Equals(object? obj)
        {
            return obj is PropObjRef other 
                && other.OwnerId == OwnerId 
                && other.EntityId == EntityId 
                && other.Lifespan == Lifespan;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    //public class PropObjRef<T> : ILifecycle
    //    where T : class
    //{
    //    protected RpgGraph Graph { get; private set; }
    //    [JsonIgnore] public LifecycleExpiry Expiry { get => LifecycleExpiry.Active; }

    //    protected class InternalRef<T>
    //        where T : class
    //    {
    //        [JsonProperty] public SpanOfTime Lifespan { get; set; }
    //        [JsonProperty] internal T Obj { get; set; }

    //        [JsonConstructor] InternalRef() { }

    //        public InternalRef(T obj, SpanOfTime lifespan)
    //        {
    //            Obj = obj;
    //            Lifespan = lifespan;
    //        }

    //        public InternalRef(T obj, PointInTime start, PointInTime end)
    //            : this(obj, new SpanOfTime(start, end))
    //                => Obj = obj;
    //    }

    //    [JsonProperty] private List<InternalRef<T>> Refs { get; set; } = new();

    //    public static implicit operator PropObjRef<T>(T obj) => new PropObjRef<T>(obj);

    //    public PropObjRef() { }

    //    public PropObjRef(T obj)
    //    {
    //        Set(obj);
    //    }

    //    public PropObjRef(RpgGraph graph)
    //    {
    //        OnCreating(graph, null);
    //        OnTimeBegins();
    //    }

    //    public PropObjRef(RpgGraph graph, T obj)
    //        : this(graph)
    //    {
    //        Set(obj);
    //    }

    //    public virtual T? Get()
    //        => Graph != null
    //            ? Refs.LastOrDefault(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active)?.Obj
    //            : Refs.LastOrDefault(x => x.Lifespan.GetExpiry(PointInTimeType.TimeBegins) == LifecycleExpiry.Active)?.Obj;

    //    public virtual T[] GetMany()
    //        => Graph != null
    //            ? Refs.Where(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active).Select(x => x.Obj).ToArray()
    //            : Refs.Where(x => x.Lifespan.GetExpiry(PointInTimeType.TimeBegins) == LifecycleExpiry.Active).Select(x => x.Obj).ToArray();

    //    private SpanOfTime GetLifespan(SpanOfTime? lifespan = null)
    //    {
    //        if (lifespan != null)
    //            lifespan.SetStartTime(Graph.Time.Now);
    //        else 
    //            lifespan = new SpanOfTime(Graph?.Time.Now ?? PointInTimeType.TimeBegins, PointInTimeType.TimeEnds, true);

    //        return lifespan;
    //    }

    //    public virtual void Set(T? obj, SpanOfTime? lifespan = null)
    //    {
    //        lifespan = GetLifespan(lifespan);
    //        if (Graph != null)
    //        {
    //            foreach (var intRef in Refs.Where(x => x.Lifespan.End.Type == PointInTimeType.TimeEnds))
    //            {
    //                if (intRef.Obj == obj && intRef.Lifespan.End >= lifespan.Start)
    //                    intRef.Lifespan = new SpanOfTime(intRef.Lifespan.Start, lifespan.End);
    //                else
    //                    intRef.Lifespan = new SpanOfTime(intRef.Lifespan.Start, Graph.Time.Now);
    //            }
    //        }

    //        if (obj != null && Get() != obj)
    //            Refs.Add(new InternalRef<T>(obj, lifespan));
    //    }

    //    public void OnCreating(RpgGraph graph, RpgObject? entity)
    //        => Graph = graph;

    //    public void OnRestoring(RpgGraph graph, RpgObject? entity)
    //        => Graph = graph;

    //    public void OnTimeBegins()
    //    { }

    //    public LifecycleExpiry OnStartLifecycle()
    //        => CalculateExpiry();

    //    public LifecycleExpiry OnUpdateLifecycle()
    //        => CalculateExpiry();

    //    private List<InternalRef<T>> GetConsolidatedRefs()
    //    {
    //        var now = Graph.Time.Now;
    //        var updatedRefs = Refs.Where(x =>
    //        {
    //            //If the encounter ends and there are refs that should continue to live after the encounter, change the lifespan start to TimePassing
    //            // so they do not expire.
    //            if (now == PointInTimeType.EncounterEnds && x.Lifespan.Start.Type == PointInTimeType.Turn && x.Lifespan.End == PointInTimeType.TimeEnds)
    //                x.Lifespan = new SpanOfTime(PointInTimeType.TimePassing, PointInTimeType.TimeEnds);

    //            var expiry = x.Lifespan.GetExpiry(Graph.Time.Now);
    //            return expiry != LifecycleExpiry.Expired && expiry != LifecycleExpiry.Destroyed;
    //        })
    //        .ToList();

    //        return updatedRefs;
    //    }

    //    protected LifecycleExpiry CalculateExpiry()
    //    {
    //        var now = Graph.Time.Now;
    //        if (!now.IsEncounterTime)
    //            Refs = GetConsolidatedRefs();

    //        var idx = Refs.FindIndex(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active);
    //        if (idx >= 0)
    //            return LifecycleExpiry.Active;

    //        if (Refs.Count() == 0)
    //            return LifecycleExpiry.Unset;

    //        if (Refs.Any(x => x.Lifespan.Start > Graph.Time.Now))
    //            return LifecycleExpiry.Pending;

    //        return Graph.Time.Now.IsEncounterTime
    //            ? LifecycleExpiry.Expired
    //            : LifecycleExpiry.Destroyed;
    //    }


    //    public override string ToString()
    //    {
    //        var item = Refs.FirstOrDefault(x => x.Lifespan.GetExpiry(Graph.Time.Now) == LifecycleExpiry.Active);
    //        return item != null
    //            ? $"[{item.Lifespan}] {item.Obj}"
    //            : $"[Unset] null";
    //    }
    //}
}
