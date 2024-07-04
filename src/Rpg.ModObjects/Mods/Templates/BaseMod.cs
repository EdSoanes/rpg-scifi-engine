using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class BaseMod : ModTemplate
    {
        public BaseMod()
        {
            Behavior = new Add(ModType.Base);
            Lifecycle = new PermanentLifecycle();
        }
    }
}
