using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Time : Mod
    {
        [JsonConstructor] protected Time()
            : base() { }

        public Time(SpanOfTime lifespan) 
            : base()
        {
            Lifespan = lifespan;
            Behavior = new Add();
        }

        public Time(PointInTimeType start, PointInTimeType end)
            : this(new SpanOfTime(start, end))
        { }

        public Time(int startTurn, int duration)
            : this(new SpanOfTime(startTurn, duration))
        { }
    }
}
