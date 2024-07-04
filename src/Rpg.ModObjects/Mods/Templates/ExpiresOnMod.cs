using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class ExpiresOnMod : ModTemplate
    {
        public ExpiresOnMod(int value)
        {
            SetLifecycle(new PermanentLifecycle());
            SetBehavior(new ExpiresOn(value));
        }
    }
}
