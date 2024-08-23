using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Time : Mod
    {
        [JsonConstructor] protected Time()
            : base() { }

        public Time(SpanOfTime lifespan, ModType modType = ModType.Standard) 
            : base()
        {
            Lifespan = lifespan;
            Behavior = new Add(modType);
        }

        public Time(PointInTimeType start, PointInTimeType end, ModType modType = ModType.Standard)
            : this(new SpanOfTime(start, end), modType)
        { }

        public Time(int startTurn, int duration, ModType modType = ModType.Standard)
            : this(new SpanOfTime(startTurn, duration), modType)
        { }
    }
}
