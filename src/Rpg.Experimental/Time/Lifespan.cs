using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Time
{
    public class Lifespan : ILifecycle
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? SyncToObjectId { get; private set; }

        private bool Started { get => Start.IsStarted() && End.IsStarted(); }
        [JsonProperty] public TimePoint Start { get; protected set; }
        [JsonProperty] public TimePoint End { get; protected set; }
        [JsonProperty] public TimePoint? Expired { get; private set; }
        [JsonProperty] public LifecycleExpiry Expiry { get; private set; } = LifecycleExpiry.Unset;
        [JsonProperty] public bool IsApplied { get; protected set; } = true;
        [JsonProperty] public bool IsDisabled { get; protected set; }

        [JsonConstructor]
        public Lifespan()
            : this(
                new TimePoint(TimePointType.TimeBegins),
                new TimePoint(TimePointType.TimeEnds))
        {
            Id = this.NewId();
        }

        public Lifespan(int startTurn, int duration)
            : this(
                  new TimePoint(TimePointType.Turn, startTurn),
                  new TimePoint(TimePointType.Turn, startTurn + duration))
        { }

        public Lifespan(TimePoint start, TimePoint end, bool started = false)
        {
            Start = start;
            End = end;
        }

        public Lifespan(string syncToObjectId)
        {
            SyncToObjectId = syncToObjectId;
        }

        public static bool operator ==(Lifespan? d1, Lifespan? d2) => d1?.Start == d2?.Start && d1?.End == d2?.End && d1?.Started == d2?.Started;
        public static bool operator !=(Lifespan? d1, Lifespan? d2) => d1?.Start != d2?.Start || d1?.End != d2?.End || d1?.Started != d2?.Started;

        protected bool SyncFromObject(RpgGraph graph)
        {
            var synced = false;
            if (SyncToObjectId != null)
            {
                var from = graph.RefreshObject(SyncToObjectId);
                if (from != null)
                {
                    Start = from.Start;
                    End = from.End;
                    Expired = from.Expired;
                    Expiry = from.Expiry;
                    IsApplied = from.IsApplied;
                    IsDisabled = from.IsDisabled;

                    synced = true;
                }
            }

            return synced;
        }

        public virtual void Apply(RpgGraph? graph)
            => IsApplied = true;

        public virtual void Unapply(RpgGraph? graph)
            => IsApplied = false;

        public virtual void UserEnabled(RpgGraph? graph)
            => IsDisabled = false;

        public virtual void UserDisabled(RpgGraph? graph)
            => IsDisabled = true;
        
        public bool OverlapsWith(Lifespan other)
        {
            if (Start <= other.Start && End > other.Start)
                return true;

            if (other.Start <= Start && other.End > Start)
                return true;

            return false;
        }

        public virtual void Expire(RpgGraph graph, TimePoint expiryTime)
        {
            Expired = expiryTime;
            OnTimeEvent(graph);
        }

        public virtual void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);

        public virtual void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId) { }

        public virtual void OnCreating(RpgGraph graph, RpgObject? obj) { }

        public virtual void OnTimeEvent(RpgGraph graph)
        {
            if (!SyncFromObject(graph))
            {
                if (!Started)
                {
                    if (Start.Type == TimePointType.Turn)
                        Start = new TimePoint(Start.Type, Start.Count + graph.Time.Now.Count);

                    if (End.Type == TimePointType.Turn)
                        End = new TimePoint(End.Type, End.Count + graph.Time.Now.Count);
                }

                Expiry = CalculateExpiry(graph, Start, Expired ?? End);

                if (!graph.Time.Now.IsEncounterTime && Expiry == LifecycleExpiry.Expired)
                    Expiry = LifecycleExpiry.Destroyed;

                if (Expiry == LifecycleExpiry.Active && (!IsApplied || IsDisabled))
                    Expiry = LifecycleExpiry.Suspended;
            }
        }

        private static LifecycleExpiry CalculateExpiry(RpgGraph graph, TimePoint start, TimePoint end)
        {
            if (start == TimePointType.Waiting && end == TimePointType.TimePasses && graph.Time.Now != TimePointType.Waiting)
                return LifecycleExpiry.Destroyed;

            if (graph.Time.Now.Type == TimePointType.TimePasses && end.Type == TimePointType.TimePasses && graph.Time.Now.Count >= end.Count)
                return LifecycleExpiry.Expired;

            if (graph.Time.Now.Type == TimePointType.Waiting && end.Type == TimePointType.Waiting && graph.Time.Now.Count >= end.Count)
                return LifecycleExpiry.Expired;

            if (start.IsEncounterTime && end.IsAfterEncounterTime && graph.Time.Now.Type == TimePointType.EncounterEnds)
                start = TimePointType.Waiting;

            if (start.IsEncounterTime && end.IsAfterEncounterTime && !graph.Time.Now.IsEncounterTime)
                return LifecycleExpiry.Expired;

            if (start > graph.Time.Now)
                return LifecycleExpiry.Pending;

            if (start <= graph.Time.Now && end > graph.Time.Now)
                return LifecycleExpiry.Active;

            return LifecycleExpiry.Expired;
        }

        public override string ToString()
        {
            return $"{Start}=>{Expired ?? End} ({Expiry})";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is Lifespan lifespan)
                return lifespan.Start == Start && lifespan.End == End && lifespan.Started == Started;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
