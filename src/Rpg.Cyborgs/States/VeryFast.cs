using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class VeryFast : State<Actor>
    {
        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions > 10;
    }
}
