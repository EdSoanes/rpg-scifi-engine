using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;

namespace Rpg.ModObjects.Mods.Templates
{
    public class HighestMod : ModTemplate
    {
        public HighestMod()
        {
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new Highest());
        }
    }
}
