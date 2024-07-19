using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeAttack : ModObjects.Actions.Action<MeleeWeapon>
    {
        [JsonConstructor] private MeleeAttack() { }

        public MeleeAttack(MeleeWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public ModSet OnCost(Actor initiator, int focusPoints)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(initiator, x => x.CurrentFocusPoints, -focusPoints)
                .Add(initiator, x => x.CurrentActionPoints, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, MeleeWeapon owner, Actor initiator, int targetDefence, int focusPoints, int? abilityScore)
        {
            var modSet = actionInstance
                .CreateActionSet()
                .DiceRoll(initiator, "Base", "2d6")
                .DiceRoll(initiator, owner, x => x.HitBonus)
                .Target(initiator, "Base", targetDefence);

            if (abilityScore != null)
                modSet.DiceRoll(initiator, "Ability", abilityScore.Value * focusPoints + 1);
            else
                modSet.DiceRoll(initiator, "MeleeAttack", initiator.MeleeAttack.Value * focusPoints + 1);

            return modSet;
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            var meleeAttacking = owner.CreateStateInstance(nameof(MeleeAttacking));
            var damage = actionInstance
                .CreateOutcomeSet()
                .Outcome(initiator, owner, x => x.Damage)
                .Outcome(initiator, x => x.Strength);

            return [meleeAttacking, damage];
        }
    }
}
