using Newtonsoft.Json;

namespace Rpg.Experimental.Time
{
    public struct TimePoint
    {
        public TimePointType Type { get; set; } = TimePointType.BeforeTime;
        public int Count { get; set; } = 0;

        /// <summary>
        /// The name of the time event when Type is Event, e.g. "Sunrise"
        /// </summary>
        public string? Event { get; set; } = null;

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

        /// <summary>
        /// The next time the named time event happens. As the start of a lifespan it means "not until the event".
        /// As the end of a lifespan it means "until the event".
        /// </summary>
        public static TimePoint AtEvent(string eventName)
            => new TimePoint(TimePointType.Event) { Event = eventName };

        [JsonIgnore] public bool IsEncounterTime { get => Type == TimePointType.Turn || Type == TimePointType.EncounterBegins; }
        [JsonIgnore] public bool IsAfterEncounterTime { get => Type == TimePointType.TimeEnds; }

        public bool IsEvent(string eventName)
            => Type == TimePointType.Event && Event == eventName;

        public static implicit operator TimePoint(TimePointType type) => new TimePoint(type);
        public static implicit operator TimePoint(int count) => new TimePoint(count);

        public static bool operator ==(TimePoint d1, TimePoint d2) => d1.Type == d2.Type && d1.Count == d2.Count && d1.Event == d2.Event;
        public static bool operator !=(TimePoint d1, TimePoint d2) => !(d1 == d2);

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
                return tp == this;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Count, Event);
        }

        public override string ToString()
        {
            if (Type == TimePointType.Turn)
                return $"{Type}:{Count}";

            if (Type == TimePointType.Event)
                return $"{Type}:{Event}";

            return Type.ToString();
        }

        public static TimePoint FromString(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return new TimePoint();

            var parts = str?.Split(':') ?? [];
            var type = parts.Length > 0
                ? Enum.Parse<TimePointType>(parts[0])
                : TimePointType.BeforeTime;

            if (type == TimePointType.Event)
                return AtEvent(parts.Length == 2 ? parts[1] : string.Empty);

            var count = parts.Length == 2
                ? int.Parse(parts[1])
                : 0;

            return new TimePoint(type, count);
        }
    }
}
