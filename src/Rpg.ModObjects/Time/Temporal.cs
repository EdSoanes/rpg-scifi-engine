﻿using System;
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

    public struct PointInTime
    {
        public PointInTimeType Type { get; set; } = PointInTimeType.BeforeTime;
        public int Count { get; set; } = 0;

        public PointInTime(PointInTimeType type)
        {
            Type = type;
        }

        public PointInTime(PointInTimeType type, int count)
            : this(type)
        {
            Count = count;
        }

        public bool IsEncounterTime { get => Type == PointInTimeType.Turn || Type == PointInTimeType.EncounterBegins || Type == PointInTimeType.EncounterEnds; }

        public static bool operator ==(PointInTime d1, PointInTime d2) => d1.Type == d2.Type && d1.Count == d2.Count;
        public static bool operator !=(PointInTime d1, PointInTime d2) => d1.Type != d2.Type || d1.Count != d2.Count;

        public static bool operator >(PointInTime d1, PointInTime d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count > d2.Count);
        public static bool operator <(PointInTime d1, PointInTime d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count < d2.Count);

        public static bool operator >=(PointInTime d1, PointInTime d2) => d1.Type >= d2.Type || (d1.Type == d2.Type && d1.Count >= d2.Count);
        public static bool operator <=(PointInTime d1, PointInTime d2) => d1.Type <= d2.Type || (d1.Type == d2.Type && d1.Count <= d2.Count);

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
    }

    public class Temporal
    {
        public enum TimeType
        {
            Passing,
            Ticking
        }

        public enum TimePassing
        {
            Turn,
            Minute,
            Hour,
            Day,
        }

        public enum EncounterEvent
        {
            Begins = 0,
            Ends = int.MaxValue,
        }

        public PointInTime Current { get; private set; } = new PointInTime(PointInTimeType.BeforeTime);

        public Temporal()
        {

        }

        public event NotifyTemporalEventHandler? OnTemporalEvent;

        public void TriggerEvent(PointInTime pointInTime)
        {
            Current = pointInTime;
            OnTemporalEvent?.Invoke(this, new TemporalEventArgs(Current));
        }

        public void Transition(PointInTime to)
        {
            if (Current == to)
                return;

            if (!Current.IsEncounterTime && !to.IsEncounterTime && to < Current)
                throw new InvalidOperationException($"Cannot transition from '{Current}' to '{to}'");

            if (Current.IsEncounterTime && to.Type < PointInTimeType.TimePassing)
                throw new InvalidOperationException($"Cannot transition from '{Current}' to '{to}'");

            if (Current.Type == PointInTimeType.BeforeTime)
            {
                Current = new PointInTime(PointInTimeType.TimeBegins);
                TriggerEvent(Current);
                Transition(to);
            }

            if (Current.Type == PointInTimeType.TimeBegins)
            {
                Current = new PointInTime(PointInTimeType.TimePassing);
                TriggerEvent(Current); 
                Transition(to);
            }

            if (Current.Type == PointInTimeType.TimePassing && to.IsEncounterTime)
            {
                Current = new PointInTime(PointInTimeType.EncounterBegins);
                TriggerEvent(Current);

                Current = new PointInTime(PointInTimeType.Turn, 1);
                TriggerEvent(Current);
                Transition(to);
            }

        }

        public void Transition(PointInTime to, PointInTime from)
        {
            switch (from.Type)
            {
                case PointInTimeType.BeforeTime: FromBeforeTime(from, to); break;
                case PointInTimeType.TimeBegins: FromTimeBegins(from,to); break;
                case PointInTimeType.TimePassing: FromTimeBegun(from, to); break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            };
        }

        public void FromBeforeTime(PointInTime from, PointInTime to)
        {
            switch (to.Type)
            {
                case PointInTimeType.BeforeTime:
                    break;
                case PointInTimeType.TimeBegins:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    break;
                case PointInTimeType.TimePassing:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.EncounterBegins:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    break;
                case PointInTimeType.EncounterEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.TimeEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.TimeEnds));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            }
        }

        public void FromTimeBegins(PointInTime from, PointInTime to)
        {
            switch (to.Type)
            {
                case PointInTimeType.TimePassing:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.EncounterBegins:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    break;
                case PointInTimeType.EncounterEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.TimeEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.TimeEnds));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            }
        }

        public void FromTimeBegun(PointInTime from, PointInTime to)
        {
            switch (to.Type)
            {
                case PointInTimeType.EncounterBegins:
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    break;
                case PointInTimeType.EncounterEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.Turn:

                    break;
                case PointInTimeType.TimeEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeEnds));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            }
        }

        public void FromEncounterBegins(PointInTime from, PointInTime to)
        {
            switch (to.Type)
            {
                case PointInTimeType.EncounterEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.TimeEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.TimeBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.TimeEnds));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            }
        }

        public void FromTurn(PointInTime from, PointInTime to)
        {
            switch (to.Type)
            {
                case PointInTimeType.EncounterBegins:
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterBegins));
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, 1));
                    break;
                case PointInTimeType.Turn:
                    TriggerEvent(new PointInTime(PointInTimeType.Turn, to.Count));
                    break;
                case PointInTimeType.EncounterEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    break;
                case PointInTimeType.TimeEnds:
                    TriggerEvent(new PointInTime(PointInTimeType.EncounterEnds));
                    TriggerEvent(new PointInTime(PointInTimeType.TimePassing));
                    TriggerEvent(new PointInTime(PointInTimeType.TimeEnds));
                    break;
                default:
                    throw new InvalidOperationException($"Cannot transition from '{from}' to '{to}'");
            }
        }
    }
}