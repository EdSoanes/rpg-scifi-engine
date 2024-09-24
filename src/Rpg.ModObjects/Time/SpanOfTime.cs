using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Time
{
    public class SpanOfTime
    {
        private bool Started { get => Start.IsStarted() && End.IsStarted(); }
        [JsonInclude] public PointInTime Start { get; private set; }
        [JsonInclude] public PointInTime End { get; private set; }

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
            if (Start.IsEncounterTime && End.IsAfterEncounterTime && now.Type == PointInTimeType.EncounterEnds)
                Start = PointInTimeType.TimePassing;

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
    }
}
