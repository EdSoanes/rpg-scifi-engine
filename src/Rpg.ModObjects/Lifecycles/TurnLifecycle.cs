using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Lifecycles
{
    public class TurnLifecycle : TimeLifecycle
    {
        private int Delay { get; set; }
        private int Duration { get; set; } = 1;

        public TurnLifecycle()
            => Duration = 1;

        public TurnLifecycle(int duration)
            => Duration = duration;

        public TurnLifecycle(int delay, int duration)
        {
            Delay = delay;
            Duration = duration;
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            if (Lifespan.Infinity)
            {
                var start = Math.Max(1, Graph.Time.Current.Count) + Delay;
                Lifespan = new SpanOfTime(start, Duration);
            }

            return base.OnStartLifecycle();
        }
    }
}
