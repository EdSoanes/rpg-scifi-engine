using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TemporalEventArgs : EventArgs
    {
        public PointInTime Time { get; private set; }

        public TemporalEventArgs(PointInTime time)
        {
            Time = time;
        }
    }

    public delegate void NotifyTemporalEventHandler(object? sender, TemporalEventArgs e);

    public enum PointInTimeType
    {
        BeforeTime,
        TimeBegins,
        TimePassing,

        EncounterBegins,
        Turn, //Count
        EncounterEnds,
        Minute,
        Hour,
        Day,

        TimeEnds
    }

    public class SpanOfTime
    {
        [JsonProperty] private bool Started { get; set; }
        [JsonProperty] public PointInTime Start { get; private set; }
        [JsonProperty] public PointInTime End { get; private set; }

        public bool Infinity { get => Start.Type <= PointInTimeType.TimeBegins && End.Type == PointInTimeType.TimeEnds; }

        [JsonConstructor] public SpanOfTime()
            : this(
                new PointInTime(PointInTimeType.TimeBegins),
                new PointInTime(PointInTimeType.TimeEnds))
        { }

        public SpanOfTime(PointInTimeType start, PointInTimeType end)
            : this(
                new PointInTime(start),
                new PointInTime(end))
        { }

        public SpanOfTime(PointInTimeType start, int endTurn)
            : this(
                new PointInTime(start),
                new PointInTime(endTurn))
        { }

        public SpanOfTime(PointInTime start, PointInTimeType end)
            : this(
                start,
                new PointInTime(end))
        { }

        public SpanOfTime(PointInTime start, int endTurn)
            : this(
                start,
                new PointInTime(endTurn))
        { }
        public SpanOfTime(int startTurn, int duration)
            : this(
                  new PointInTime(PointInTimeType.Turn, startTurn),
                  new PointInTime(PointInTimeType.Turn, startTurn + duration))
        { }

        public SpanOfTime(PointInTime start, PointInTime end)
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

                Started = true;
            }
        }

        public LifecycleExpiry GetExpiry(PointInTime now)
        {
            if (Start.Type == PointInTimeType.Turn && End.Type == PointInTimeType.Turn && !now.IsEncounterTime)
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
            return $"{Start} => {End}";
        }
    }

    public struct PointInTime
    {
        public PointInTimeType Type { get; set; } = PointInTimeType.BeforeTime;
        public int Count { get; set; } = 0;

        public PointInTime(PointInTimeType type)
        {
            Type = type;
        }

        public PointInTime(int count)
            : this(PointInTimeType.Turn, count)
        { }

        public PointInTime(PointInTimeType type, int count)
            : this(type)
        {
            Count = count;
        }

        public bool IsEncounterTime { get => Type == PointInTimeType.Turn || Type == PointInTimeType.EncounterBegins; }

        public static bool operator ==(PointInTime d1, PointInTime d2) => d1.Type == d2.Type && d1.Count == d2.Count;
        public static bool operator !=(PointInTime d1, PointInTime d2) => d1.Type != d2.Type || d1.Count != d2.Count;

        public static bool operator >(PointInTime d1, PointInTime d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count > d2.Count);
        public static bool operator <(PointInTime d1, PointInTime d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count < d2.Count);

        public static bool operator >=(PointInTime d1, PointInTime d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count >= d2.Count);
        public static bool operator <=(PointInTime d1, PointInTime d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count <= d2.Count);

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is PointInTime tp)
                return tp.Type == Type && tp.Count == Count;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Type == PointInTimeType.Turn ? Count.ToString() : Type.ToString();
        }
    }

    public class Temporal
    {
        [JsonProperty] public PointInTime Current { get; private set; } = new PointInTime(PointInTimeType.BeforeTime);

        public Temporal()
        {

        }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public void TriggerEvent()
            => TriggerEvent(Current);

        public void TriggerEvent(PointInTime pointInTime)
        {
            Current = pointInTime;
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Current));
        }

        public void TriggerEvent(PointInTimeType type, int count = 0)
        {
            Current = new PointInTime(type, count);
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Current));
        }

        public void Transition(PointInTimeType type, int count = 0)
            => Transition(new PointInTime(type, type == PointInTimeType.Turn ? Math.Max(count, 1) : count));

        public void Transition(PointInTime to)
        {
            if (to == Current)
                return;

            if (!Current.IsEncounterTime && !to.IsEncounterTime && to < Current)
                throw new InvalidOperationException($"Cannot transition from '{Current}' to '{to}'");

            if (Current.IsEncounterTime && to.Type < PointInTimeType.TimePassing)
                throw new InvalidOperationException($"Cannot transition from '{Current}' to '{to}'");

            if (Current.Type == PointInTimeType.BeforeTime)
            {
                TriggerEvent(PointInTimeType.BeforeTime);
                TriggerEvent(PointInTimeType.TimeBegins);
                Transition(to);
                return;
            }

            if (Current.Type == PointInTimeType.TimeBegins)
            {
                TriggerEvent(PointInTimeType.TimePassing);
                Transition(to);
                return;
            }

            if (!to.IsEncounterTime && to.Type != PointInTimeType.EncounterEnds)
            {
                TriggerEvent(to);
                if (to.Type != PointInTimeType.TimeEnds && to.Type != PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.TimePassing);
                return;
            }

            if (to.Type == PointInTimeType.EncounterBegins)
            {
                if (Current.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.EncounterEnds);

                if (Current.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                TriggerEvent(PointInTimeType.EncounterBegins);
                TriggerEvent(PointInTimeType.Turn, 1);
                return;
            }

            if (to.Type == PointInTimeType.Turn)
            {
                if (Current.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                if (Current.Type == PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.EncounterBegins);

                if (Current.Type == PointInTimeType.EncounterBegins || Current.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.Turn, to.Count);

                return;
            }

            if (to.Type == PointInTimeType.EncounterEnds)
            {
                if (Current.Type == PointInTimeType.TimePassing)
                    TriggerEvent(PointInTimeType.EncounterBegins);

                if (Current.Type == PointInTimeType.EncounterBegins || Current.Type == PointInTimeType.Turn)
                    TriggerEvent(PointInTimeType.EncounterEnds);

                if (Current.Type == PointInTimeType.EncounterEnds)
                    TriggerEvent(PointInTimeType.TimePassing);

                return;
            }
        }
    }
}
