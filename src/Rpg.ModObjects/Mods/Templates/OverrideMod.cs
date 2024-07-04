using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class OverrideMod : ModTemplate
    {
        public OverrideMod()
        {
            Behavior = new Replace(ModType.Override);
            Lifecycle = new PermanentLifecycle();
        }
    }
}
