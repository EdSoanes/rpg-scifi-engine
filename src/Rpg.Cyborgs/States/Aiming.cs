using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class Aiming : State<Actor>
    {
        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions > 10;

        protected override void WhenOn(Actor owner)
        {
            this.Lifecycle.
            Mod(owner, x => x.RangedAimBonus, )
        }
    }
}
