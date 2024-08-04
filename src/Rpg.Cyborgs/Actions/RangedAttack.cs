using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
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

        public bool OnCanAct(Activity activity, RangedWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool OnCost(Activity activity, Actor initiator)
        {
            activity.CostSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(Activity activity, RangedWeapon owner, Actor initiator, int targetDefence, int focusPoints, int? abilityScore)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(initiator, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActivityMod("diceRoll", "Base", "2d6")
                .ActivityMod("diceRoll", owner, x => x.HitBonus)
                .ActivityMod("target", "Base", targetDefence);

            if (abilityScore != null)
                activity.ActivityMod("diceRoll", "Ability", abilityScore.Value * (focusPoints + 1));
            else
                activity.ActivityMod("diceRoll", initiator, x => x.RangedAttack.Value * (focusPoints + 1));

            return true;
        }

        public bool OnOutcome(Activity activity, RangedWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage);

            var rangedAttacking = owner.CreateStateInstance(nameof(RangedAttacking));
            activity.OutputSets.Add(rangedAttacking);

            return true;
        }
    }
}
