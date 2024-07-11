using Newtonsoft.Json;

namespace Rpg.ModObjects.Time
{
    public struct TimePoint
    {
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public int Tick { get; private set; }

        public TimePoint(string type, int tick) 
        {
            Type = type;
            Tick = tick;
        }

        public static bool operator ==(TimePoint d1, TimePoint d2) => d1.Type == d2.Type && d1.Tick == d2.Tick;
        public static bool operator !=(TimePoint d1, TimePoint d2) => d1.Type != d2.Type || d1.Tick != d2.Tick;

        public static bool operator >(TimePoint d1, TimePoint d2) => d1.Tick > d2.Tick;
        public static bool operator <(TimePoint d1, TimePoint d2) => d1.Tick < d2.Tick;

        public static bool operator >=(TimePoint d1, TimePoint d2) => d1.Tick >= d2.Tick;
        public static bool operator <=(TimePoint d1, TimePoint d2) => d1.Tick <= d2.Tick;

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is TimePoint tp)
                return tp.Type == Type && tp.Tick == Tick;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
