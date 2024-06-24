using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;

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
                delay == 0 ? TimePoints.Empty : TimePoints.Encounter(delay), 
                TimePoints.Encounter(duration));

            Behavior = new Add(ModType.Standard);
        }
    }

    public class EncounterMod : ModTemplate
    {
        public EncounterMod()
        {
            Lifecycle = new TimeLifecycle(
                TimePoints.Empty,
                TimePoints.EndOfEncounter);

            Behavior = new Add(ModType.Standard);
        }
    }
}
