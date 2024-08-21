using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time.Templates
{
    public class TurnMod : ModTemplate
    {
        public TurnMod()
            : this(0, 1) { }
        public TurnMod(int duration)
            : this(0, duration) { }

        public TurnMod(int delay, int duration)
        {
            Lifecycle = new TurnLifecycle(delay, duration);
            Behavior = new Add(ModType.Standard);
        }
    }
}
