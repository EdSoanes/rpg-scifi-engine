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
}
