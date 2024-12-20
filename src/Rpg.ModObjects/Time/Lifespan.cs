﻿using Newtonsoft.Json;

namespace Rpg.ModObjects.Time
{
    public class Lifespan
    {
        private bool Started { get => Start.IsStarted() && End.IsStarted(); }
        [JsonProperty] public PointInTime Start { get; private set; }
        [JsonProperty] public PointInTime End { get; private set; }
        [JsonProperty] public PointInTime? Expired { get; private set; }
        [JsonProperty] public LifecycleExpiry Expiry { get; private set; } = LifecycleExpiry.Unset;

        [JsonConstructor]
        public Lifespan()
            : this(
                new PointInTime(PointInTimeType.TimeBegins),
                new PointInTime(PointInTimeType.TimeEnds))
        { }

        public Lifespan(int startTurn, int duration)
            : this(
                  new PointInTime(PointInTimeType.Turn, startTurn),
                  new PointInTime(PointInTimeType.Turn, startTurn + duration))
        { }

        public Lifespan(PointInTime start, PointInTime end, bool started = false)
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

        public LifecycleExpiry UpdateExpiry(PointInTime now)
        {
            if (Expired != null)
                Expiry = new Lifespan(PointInTimeType.BeforeTime, Expired.Value).UpdateExpiry(now);
            else
                Expiry = CalculateExpiry(now);

            if (Expiry == LifecycleExpiry.Expired && !now.IsEncounterTime)
                Expiry = LifecycleExpiry.Destroyed;

            return Expiry;
        }

        private LifecycleExpiry CalculateExpiry(PointInTime now)
        {
            if (Start == PointInTimeType.Waiting && End == PointInTimeType.TimePasses && now != PointInTimeType.Waiting)
                return LifecycleExpiry.Destroyed;

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
