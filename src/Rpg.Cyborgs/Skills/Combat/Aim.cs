using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
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

        public bool OnCanAct(Actor owner)
            => !owner.IsStateOn(nameof(Aiming)) || owner.RangedAimBonus < 6;

        public ModSet OnCost(Actor owner, Actor initiator, int focusPoints)
        {
            return new ModSet(owner.Id, new TurnLifecycle());
        }

        public ActionModSet OnAct(ActionInstance actionInstance)
        {
            var modSet = actionInstance.CreateActionSet();
            return modSet;
        }

        public ModSet[] OnOutcome(Actor owner)
        {
            var aiming = owner.CreateStateInstance(nameof(Aiming));
            return [aiming];
        }
    }
}
