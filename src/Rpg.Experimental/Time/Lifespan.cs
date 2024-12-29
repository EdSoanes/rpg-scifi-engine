using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Time
{
    public class Lifespan : ILifecycle
    {
        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; protected set; }
        [JsonProperty] public bool SyncToOwner { get; protected set; }
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

        public Lifespan(string ownerId, bool syncToOwner)
        {
            OwnerId = ownerId;
            SyncToOwner = syncToOwner;
        }

        public Lifespan(string ownerId, int startTurn, int duration)
            : this(
                  ownerId,
                  new TimePoint(TimePointType.Turn, startTurn),
                  new TimePoint(TimePointType.Turn, startTurn + duration))
        { }

        public Lifespan(string ownerId, TimePoint start, TimePoint end, bool started = false)
            : this(start, end, started)
        {
            OwnerId = ownerId;
        }

        public static bool operator ==(Lifespan? d1, Lifespan? d2) => d1?.Start == d2?.Start && d1?.End == d2?.End && d1?.Started == d2?.Started;
        public static bool operator !=(Lifespan? d1, Lifespan? d2) => d1?.Start != d2?.Start || d1?.End != d2?.End || d1?.Started != d2?.Started;

        protected bool SyncLifespanFromOwner(RpgGraph graph)
        {
            var synced = false;
            if (OwnerId != null && SyncToOwner)
            {
                var from = graph.RefreshObject(OwnerId);
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
            if (!SyncLifespanFromOwner(graph))
            {
                if (!Started)
                {
                    if (Start.Type == TimePointType.Turn)
                        Start = new TimePoint(Start.Type, Start.Count + graph.Time.Now.Count);

                    if (End.Type == TimePointType.Turn)
                        End = new TimePoint(End.Type, End.Count + graph.Time.Now.Count);
                }

                Expiry = CalculateExpiry(graph, Start, Expired ?? End);
            }
        }

        protected virtual LifecycleExpiry CalculateExpiry(RpgGraph graph, TimePoint start, TimePoint end)
        {
            var expiry = LifecycleExpiry.Expired;

            if (start == TimePointType.Waiting && end == TimePointType.TimePasses && graph.Time.Now != TimePointType.Waiting)
                expiry = LifecycleExpiry.Destroyed;

            else if (graph.Time.Now.Type == TimePointType.TimePasses && end.Type == TimePointType.TimePasses && graph.Time.Now.Count >= end.Count)
                expiry = LifecycleExpiry.Expired;

            else if (graph.Time.Now.Type == TimePointType.Waiting && end.Type == TimePointType.Waiting && graph.Time.Now.Count >= end.Count)
                expiry = LifecycleExpiry.Expired;

            else
            {
                if (start.IsEncounterTime && end.IsAfterEncounterTime && graph.Time.Now.Type == TimePointType.EncounterEnds)
                    start = TimePointType.Waiting;

                if (start.IsEncounterTime && end.IsAfterEncounterTime && !graph.Time.Now.IsEncounterTime)
                    expiry = LifecycleExpiry.Expired;

                else if (start > graph.Time.Now)
                    expiry = LifecycleExpiry.Pending;

                else if (start <= graph.Time.Now && end > graph.Time.Now)
                    expiry = LifecycleExpiry.Active;

                else
                    expiry = LifecycleExpiry.Expired;

            }

            if (!graph.Time.Now.IsEncounterTime && expiry == LifecycleExpiry.Expired)
                expiry = LifecycleExpiry.Destroyed;

            if (expiry == LifecycleExpiry.Active && (!IsApplied || IsDisabled))
                expiry = LifecycleExpiry.Suspended;

            return expiry;
        }

        public override string ToString()
        {
            var expiry = Expiry.ToString();
            if (OwnerId != null)
                expiry += $"{expiry} from {OwnerId}";

            return $"{Start}=>{Expired ?? End} ({expiry})";
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
