using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
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

        public ModSet OnCost(Actor owner, Actor initiator, int focusPoints)
        {
            return new ModSet(owner.Id, new TurnLifecycle());
        }

        public ModSet[] OnAct(int actionNo, Actor owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(owner.Id, new TurnLifecycle());
            return [modSet];
        }

        public ModSet[] OnOutcome(Actor owner, int diceRoll, int targetDefence)
        {
            var aiming = owner.GetState(nameof(Aiming))!.CreateInstance(new TurnLifecycle());
            return [aiming];
        }
    }
}
