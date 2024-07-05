using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : State<Actor>
    {
        public MeleeAttacked(Actor owner)
           : base(owner) { }
    }
}
