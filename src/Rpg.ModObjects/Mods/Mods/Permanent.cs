using Rpg.ModObjects.Behaviors;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Permanent : Mod
    {
        public Permanent()
            : base()
        { }

        public Permanent(string ownerId)
            : base()
        {
            OwnerId = ownerId;
        }

        public Permanent(BaseBehavior behavior)
            : base(nameof(Permanent))
        {
            Behavior = behavior;
        }

        public Permanent(string ownerId, BaseBehavior behavior)
            : base(nameof(Permanent))
        {
            OwnerId = ownerId;
            Behavior = behavior;
        }

        public Permanent(string ownerId, string name, BaseBehavior behavior)
            : base(name)
        {
            OwnerId = ownerId;
            Behavior = behavior;
        }

        public Permanent(string ownerId, string name, ModScope scope)
            : base(name)
        {
            OwnerId = ownerId;
            Behavior.Scope = scope;
        }
    }
}
