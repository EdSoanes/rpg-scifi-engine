using Rpg.ModObjects.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
