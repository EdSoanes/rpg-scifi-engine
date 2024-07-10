using Newtonsoft.Json;
using Rpg.Cyborgs.Skills.Combat;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : ModObjects.Actions.Action<RangedWeapon>
    {
        [JsonConstructor] private RangedAttack() { }

        public RangedAttack(RangedWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActions > 0;

        public ModSet OnCost(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(initiator, x => x.CurrentFocusPoints, -focusPoints)
                .Add(initiator, x => x.CurrentActions, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, MeleeWeapon owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var actionModSet = actionInstance.CreateActionSet()
                .DiceRoll(initiator, "Base", "2d6")
                .DiceRoll(initiator, owner, x => x.HitBonus)
                .DiceRoll(initiator, x => x.RangedAimBonus);

            if (abilityScore != null)
                actionModSet.DiceRoll(initiator, "Ability", abilityScore.Value);
            else
                actionModSet.DiceRoll(initiator, x => x.RangedAttack);

            return actionModSet;
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            var firing = owner.CreateStateInstance(nameof(Firing));
            return [firing];
        }
    }
}
