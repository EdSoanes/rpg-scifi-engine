using Rpg.ModObjects.Behaviors;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Permanent : Mod
    {
        public Permanent()
            : base()
        { }

        public Permanent(BaseBehavior behavior)
            : base()
        {
            Behavior = behavior;
        }

        public Permanent(ModScope scope)
            : base()
        {
            Behavior.Scope = scope;
        }
    }
}
