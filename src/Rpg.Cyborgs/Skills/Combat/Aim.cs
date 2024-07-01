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

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
        {
            if (owner is Actor actor)
                return !actor.IsStateOn(nameof(Aiming)) || actor.RangedAimBonus < 6;

            return false;
        }
    }
}
