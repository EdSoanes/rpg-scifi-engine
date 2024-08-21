using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Lifecycles
{
    public class TimeLifecycle : BaseLifecycle
    {
        public TimeLifecycle() { }

        public TimeLifecycle(PointInTimeType start, PointInTimeType end)
            => Lifespan = new SpanOfTime(start, end);

        public TimeLifecycle(int startTurn, int duration)
            => Lifespan = new SpanOfTime(startTurn, duration);

        public TimeLifecycle(PointInTime start, PointInTime end)
            => Lifespan = new SpanOfTime(start, end);
    }
}
