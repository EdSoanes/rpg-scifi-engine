using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    public class Lifespan
    {
        private bool Started { get => Start.IsStarted() && End.IsStarted(); }
        [JsonProperty] public TimePoint Start { get; private set; }
        [JsonProperty] public TimePoint End { get; private set; }
        [JsonProperty] public TimePoint? Expired { get; private set; }
        [JsonProperty] public LifecycleExpiry Expiry { get; private set; } = LifecycleExpiry.Unset;

        [JsonConstructor]
        public Lifespan()
            : this(
                new TimePoint(TimePointType.TimeBegins),
                new TimePoint(TimePointType.TimeEnds))
        { }

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

        public static bool operator ==(Lifespan? d1, Lifespan? d2) => d1?.Start == d2?.Start && d1?.End == d2?.End && d1?.Started == d2?.Started;
        public static bool operator !=(Lifespan? d1, Lifespan? d2) => d1?.Start != d2?.Start || d1?.End != d2?.End || d1?.Started != d2?.Started;

        public bool OverlapsWith(Lifespan other)
        {
            if (Start <= other.Start && End > other.Start)
                return true;

            if (other.Start <= Start && other.End > Start)
                return true;

            return false;
        }

        public void SetStartTime(TimePoint now)
        {
            if (!Started)
            {
                if (Start.Type == TimePointType.Turn)
                    Start = new TimePoint(Start.Type, Start.Count + now.Count);

                if (End.Type == TimePointType.Turn)
                    End = new TimePoint(End.Type, End.Count + now.Count);
            }
        }

        public virtual void Expire(TimePoint now, TimePoint expiryTime)
        {
            Expired = expiryTime;
            OnTimeEvent(now);
        }

        public virtual void Expire(TimePoint now)
        {
            Expired = now;
            OnTimeEvent(now);
        }

        public virtual void OnTimeEvent(TimePoint now)
        {
            if (Expired != null)
            {
                var newLifespan = new Lifespan(TimePointType.BeforeTime, Expired.Value);
                newLifespan.OnTimeEvent(now);
                Expiry = newLifespan.Expiry;
            }
            else
                Expiry = CalculateExpiry(now);

            if (Expiry == LifecycleExpiry.Expired && !now.IsEncounterTime)
                Expiry = LifecycleExpiry.Destroyed;
        }

        private LifecycleExpiry CalculateExpiry(TimePoint now)
        {
            if (Start == TimePointType.Waiting && End == TimePointType.TimePasses && now != TimePointType.Waiting)
                return LifecycleExpiry.Destroyed;

            if (now.Type == TimePointType.Waiting && Expiry != LifecycleExpiry.Unset)
                return Expiry;

            if (now.Type == TimePointType.TimePasses && End.Type == TimePointType.TimePasses && now.Count >= End.Count)
                return LifecycleExpiry.Expired;

            if (now.Type == TimePointType.Waiting && End.Type == TimePointType.Waiting && now.Count >= End.Count)
                return LifecycleExpiry.Expired;

            if (Start.IsEncounterTime && End.IsAfterEncounterTime && now.Type == TimePointType.EncounterEnds)
                Start = TimePointType.Waiting;

            if (Start.IsEncounterTime && End.IsAfterEncounterTime && !now.IsEncounterTime)
                return LifecycleExpiry.Expired;

            if (Start > now)
                return LifecycleExpiry.Pending;

            if (Start <= now && End > now)
                return LifecycleExpiry.Active;

            return now.IsEncounterTime
                ? LifecycleExpiry.Expired
                : LifecycleExpiry.Destroyed;
        }

        public override string ToString()
        {
            return $"{Start}=>{Expired ?? End}";
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
