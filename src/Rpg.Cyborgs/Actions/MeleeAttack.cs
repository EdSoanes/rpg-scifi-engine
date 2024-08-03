using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeAttack : ModObjects.Actions.Action<MeleeWeapon>
    {
        [JsonConstructor] private MeleeAttack() { }

        public MeleeAttack(MeleeWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(RpgActivity activity, MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool OnCost(RpgActivity activity, Actor initiator)
        {
            activity.CostSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(RpgActivity activity, MeleeWeapon owner, Actor initiator, int targetDefence, int focusPoints, int? abilityScore)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(initiator, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActionMod("diceRoll", "Base", "2d6")
                .ActionMod("diceRoll", owner, x => x.HitBonus)
                .ActionMod("target", "Base", targetDefence);

            var score = (abilityScore ?? initiator.MeleeAttack.Value) * (focusPoints + 1);

            if (abilityScore != null)
                activity.ActionMod("diceRoll", "Ability", score);
            else
                activity.ActionMod("diceRoll", "MeleeAttack.Value", score);

            return true;
        }

        public bool OnOutcome(RpgActivity activity, MeleeWeapon owner, Actor initiator, int diceRoll, int target)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage)
                .ActivityMod("damage", initiator, x => x.Strength.Value);

            var meleeAttacking = initiator.CreateStateInstance(nameof(MeleeAttacking));
            activity.OutputSets.Add(meleeAttacking);

            return true;
        }
    }
}
