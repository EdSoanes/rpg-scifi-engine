using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public abstract class Time : Mod
    {
        [JsonConstructor] protected Time()
            : base() { }

        protected Time(string name, SpanOfTime lifespan) 
            : base(name)
        {
            Lifespan = lifespan;
            Behavior = new Add();
        }

        protected Time(string name, PointInTimeType start, PointInTimeType end)
            : this(name, new SpanOfTime(start, end))
        { }

        protected Time(string name, int startTurn, int duration)
            : this(name, new SpanOfTime(startTurn, duration))
        { }
    }
}
