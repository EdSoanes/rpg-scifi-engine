using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Time
{
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

        [JsonIgnore] public bool IsEncounterTime { get => Type == PointInTimeType.Turn || Type == PointInTimeType.EncounterBegins; }
        [JsonIgnore] public bool IsAfterEncounterTime { get => Type == PointInTimeType.TimeEnds; }

        public static implicit operator PointInTime(PointInTimeType type) => new PointInTime(type);
        public static implicit operator PointInTime(int count) => new PointInTime(count);

        public static bool operator ==(PointInTime d1, PointInTime d2) => d1.Type == d2.Type && d1.Count == d2.Count;
        public static bool operator !=(PointInTime d1, PointInTime d2) => d1.Type != d2.Type || d1.Count != d2.Count;

        public static bool operator >(PointInTime d1, PointInTime d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count > d2.Count);
        public static bool operator <(PointInTime d1, PointInTime d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count < d2.Count);

        public static bool operator >=(PointInTime d1, PointInTime d2) => d1.Type > d2.Type || (d1.Type == d2.Type && d1.Count >= d2.Count);
        public static bool operator <=(PointInTime d1, PointInTime d2) => d1.Type < d2.Type || (d1.Type == d2.Type && d1.Count <= d2.Count);

        public bool IsStarted()
            => Type != PointInTimeType.Turn || Count > 0;

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
            return Type == PointInTimeType.Turn
                ? $"{Type}:{Count}"
                : Type.ToString();
        }

        public static PointInTime FromString(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return new PointInTime();

            var parts = str?.Split(':') ?? [];
            var type = parts.Length > 0
                ? Enum.Parse<PointInTimeType>(parts[0])
                : PointInTimeType.BeforeTime;

            var count = parts.Length == 2
                ? int.Parse(parts[1])
                : 0;

            return new PointInTime(type, count);
        }
    }
}
