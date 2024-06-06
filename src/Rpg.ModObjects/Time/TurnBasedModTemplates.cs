using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TurnMod : ModTemplate
    {
        public TurnMod()
            : this(0, 1) { }

        public TurnMod(int duration)
            : this(0, duration) { }

        public TurnMod(int delay, int duration)
        {
            Lifecycle = new TimeLifecycle(
                new TimePoint(nameof(TurnBasedTimeEngine.Encounter), delay),
                new TimePoint(nameof(TurnBasedTimeEngine.Encounter), duration));

            Behavior = new Add(ModType.Standard);
        }
    }

    public class EncounterMod : ModTemplate
    {
        public EncounterMod()
        {
            Lifecycle = new TimeLifecycle(
                TurnBasedTimeEngine.EncounterStart,
                TurnBasedTimeEngine.EncounterEnd);

            Behavior = new Add(ModType.Standard);
        }
    }
}
