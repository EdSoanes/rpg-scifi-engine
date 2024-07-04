using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class Moving : State<Actor>
    {
        public Moving(Actor owner)
            : base(owner) { }
    }
}
