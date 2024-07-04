using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class PermanentMod : ModTemplate
    {
        public PermanentMod(string name)
        {
            Name = name;
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new Add(ModType.Standard));
        }

        public PermanentMod()
        {
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new Add(ModType.Standard));
        }
    }
}
