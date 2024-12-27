using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    public struct TimePoint
    {
        public TimePointType Type { get; set; } = TimePointType.BeforeTime;
        public int Count { get; set; } = 0;

        public TimePoint(TimePointType type)
        {
            Type = type;
        }

        public TimePoint(int count)
            : this(TimePointType.Turn, count)
        { }

        public TimePoint(TimePointType type, int count)
            : this(type)
        {
            Count = count;
        }

        [JsonIgnore] public bool IsEncounterTime { get => Type == TimePointType.Turn || Type == TimePointType.EncounterBegins; }
        [JsonIgnore] public bool IsAfterEncounterTime { get => Type == TimePointType.TimeEnds; }

        public static implicit operator TimePoint(TimePointType type) => new TimePoint(type);
        public static implicit operator TimePoint(int count) => new TimePoint(count);

        public static bool operator ==(TimePoint d1, TimePoint d2) => d1.Type == d2.Type && d1.Count == d2.Count;
        public static bool operator !=(TimePoint d1, TimePoint d2) => d1.Type != d2.Type || d1.Count != d2.Count;

        public static bool operator >(TimePoint d1, TimePoint d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count > d2.Count);
        public static bool operator <(TimePoint d1, TimePoint d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count < d2.Count);

        public static bool operator >=(TimePoint d1, TimePoint d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count >= d2.Count);
        public static bool operator <=(TimePoint d1, TimePoint d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count <= d2.Count);

        public bool IsStarted()
            => Type != TimePointType.Turn || Count > 0;

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is TimePoint tp)
                return tp.Type == Type && tp.Count == Count;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Type == TimePointType.Turn
                ? $"{Type}:{Count}"
                : Type.ToString();
        }

        public static TimePoint FromString(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return new TimePoint();

            var parts = str?.Split(':') ?? [];
            var type = parts.Length > 0
                ? Enum.Parse<TimePointType>(parts[0])
                : TimePointType.BeforeTime;

            var count = parts.Length == 2
                ? int.Parse(parts[1])
                : 0;

            return new TimePoint(type, count);
        }
    }
}
