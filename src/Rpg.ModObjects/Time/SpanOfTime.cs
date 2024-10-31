using Newtonsoft.Json;

namespace Rpg.ModObjects.Time
{
    public class SpanOfTime
    {
        private bool Started { get => Start.IsStarted() && End.IsStarted(); }
        [JsonProperty] public PointInTime Start { get; private set; }
        [JsonProperty] public PointInTime End { get; private set; }
        [JsonProperty] public LifecycleExpiry Expiry { get; private set; } = LifecycleExpiry.Unset;

        [JsonConstructor]
        public SpanOfTime()
            : this(
                new PointInTime(PointInTimeType.TimeBegins),
                new PointInTime(PointInTimeType.TimeEnds))
        { }

        public SpanOfTime(int startTurn, int duration)
            : this(
                  new PointInTime(PointInTimeType.Turn, startTurn),
                  new PointInTime(PointInTimeType.Turn, startTurn + duration))
        { }

        public SpanOfTime(PointInTime start, PointInTime end, bool started = false)
        {
            Start = start;
            End = end;
        }

        public static bool operator ==(SpanOfTime? d1, SpanOfTime? d2) => d1?.Start == d2?.Start && d1?.End == d2?.End && d1?.Started == d2?.Started;
        public static bool operator !=(SpanOfTime? d1, SpanOfTime? d2) => d1?.Start != d2?.Start || d1?.End != d2?.End || d1?.Started != d2?.Started;

        public bool OverlapsWith(SpanOfTime other)
        {
            if (Start <= other.Start && End > other.Start)
                return true;

            if (other.Start <= Start && other.End > Start)
                return true;

            return false;
        }

        public void SetStartTime(PointInTime now)
        {
            if (!Started)
            {
                if (Start.Type == PointInTimeType.Turn)
                    Start = new PointInTime(Start.Type, Start.Count + now.Count);

                if (End.Type == PointInTimeType.Turn)
                    End = new PointInTime(End.Type, End.Count + now.Count);
            }
        }

        public LifecycleExpiry GetExpiry(PointInTime now)
        {
            Expiry = CalculateExpiry(now);
            return Expiry;
        }

        private LifecycleExpiry CalculateExpiry(PointInTime now)
        {
            if (now.Type == PointInTimeType.Waiting && Expiry != LifecycleExpiry.Unset)
                return Expiry;

            if (now.Type == PointInTimeType.TimePasses && End.Type == PointInTimeType.TimePasses && now.Count >= End.Count)
                return LifecycleExpiry.Expired;

            if (Start.IsEncounterTime && End.IsAfterEncounterTime && now.Type == PointInTimeType.EncounterEnds)
                Start = PointInTimeType.Waiting;

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
            return $"{Start}=>{End}";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is SpanOfTime lifespan)
                return lifespan.Start == Start && lifespan.End == End && lifespan.Started == Started;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
