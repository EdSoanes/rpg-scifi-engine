using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : ModObjects.Actions.Action<MeleeWeapon>
    {
        [JsonConstructor] private RangedAttack() { }

        public RangedAttack(MeleeWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(RpgActivity activity, RangedWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool OnCost(RpgActivity activity, Actor initiator)
        {
            activity.OutcomeSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(RpgActivity activity, RangedWeapon owner, Actor initiator, int targetDefence, int focusPoints, int? abilityScore)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(initiator, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActionMod("diceRoll", "Base", "2d6")
                .ActionMod("diceRoll", owner, x => x.HitBonus)
                .ActionMod("target", "Base", targetDefence);

            if (abilityScore != null)
                activity.ActionMod("diceRoll", "Ability", abilityScore.Value * (focusPoints + 1));
            else
                activity.ActionMod("diceRoll", initiator, x => x.RangedAttack.Value * (focusPoints + 1));

            return true;
        }

        public bool OnOutcome(RpgActivity activity, RangedWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage);

            var rangedAttacking = owner.CreateStateInstance(nameof(RangedAttacking));
            activity.OutcomeSets.Add(rangedAttacking);

            return true;
        }
    }
}
