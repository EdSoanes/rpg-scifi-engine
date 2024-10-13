using Rpg.ModObjects.Behaviors;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Permanent : Mod
    {
        public Permanent()
            : base()
        { }

        public Permanent(string name)
            : base(name)
        { }

        public Permanent(BaseBehavior behavior)
            : this(nameof(Permanent), behavior)
        {
        }

        public Permanent(string name, BaseBehavior behavior)
            : base(name)
        {
            Behavior = behavior;
        }

        public Permanent(string name, ModScope scope)
            : base(name)
        {
            Behavior.Scope = scope;
        }
    }
}
