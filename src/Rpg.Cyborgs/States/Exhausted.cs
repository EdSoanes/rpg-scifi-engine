using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class Exhausted : State<Actor>
    {
        protected override bool IsOnWhen(Actor owner)
            => owner.CurrentStaminaPoints == 0;
    }
}
