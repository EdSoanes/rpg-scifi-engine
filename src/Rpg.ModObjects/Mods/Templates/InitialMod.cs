using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class InitialMod : ModTemplate
    {
        public InitialMod()
        {
            Behavior = new Replace(ModType.Initial);
            Lifecycle = new PermanentLifecycle();
        }
    }
}
