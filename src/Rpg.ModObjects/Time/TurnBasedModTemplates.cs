using Rpg.ModObjects.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TurnMod : ModTemplate
    {
        public TurnMod() 
        {
            Lifecycle = Rpg.ModObjects.Time.Lifecycle.Turn();
            Behavior = new Add(ModType.Standard);
        }

        public TurnMod(int duration)
        {
            Lifecycle = Rpg.ModObjects.Time.Lifecycle.Turn(duration);
            Behavior = new Add(ModType.Standard);
        }
    }
}
