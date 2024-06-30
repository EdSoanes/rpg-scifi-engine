using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class Aim : Skill
    {
        [JsonConstructor] private Aim() { }

        public Aim(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
        {
            if (owner is Actor actor)
            {
                var isAiming = actor.GetState(nameof(Aiming))?.IsOn ?? false;
                return !isAiming || actor.RangedAimBonus < 6;
            }

            return false;
        }


    }
}
