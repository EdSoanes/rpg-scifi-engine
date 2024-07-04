using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time.Lifecycles
{
    public class TurnLifecycle : TimeLifecycle
    {
        public TurnLifecycle()
            : base(TimePoints.Encounter(1))
        { }

        public TurnLifecycle(int duration) 
            : base(TimePoints.Encounter(duration))
        { }

        public TurnLifecycle(int delay, int duration)
            : base(delay == 0 ? TimePoints.Empty : TimePoints.Encounter(delay), TimePoints.Encounter(duration))
        { }

        public override LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint time, Mod? mod = null)
        {
            Expiry = time.Type != nameof(TimePoints.Encounter)
                ? LifecycleExpiry.Expired
                : base.OnStartLifecycle(graph, time, mod);

            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint time, Mod? mod = null)
        {
            Expiry = time.Type != nameof(TimePoints.Encounter)
                ? LifecycleExpiry.Expired
                : base.OnUpdateLifecycle(graph, time, mod);

            return Expiry;
        }
    }
}
