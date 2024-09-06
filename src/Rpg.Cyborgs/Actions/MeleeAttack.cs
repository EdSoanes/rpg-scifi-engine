using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeAttack : ModObjects.Actions.Action<MeleeWeapon>
    {
        [JsonConstructor] private MeleeAttack() { }

        public MeleeAttack(MeleeWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(Activity activity, MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActionPoints > 0;

        public bool OnCost(Activity activity, Actor initiator)
        {
            activity.CostSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(Activity activity, MeleeWeapon owner, Actor initiator, int targetDefence, int focusPoints, int? abilityScore)
        {
            if (focusPoints > 0)
                activity.OutcomeSet
                    .Add(initiator, x => x.CurrentFocusPoints, -focusPoints);

            activity
                .ActivityMod("diceRoll", "Base", "2d6")
                .ActivityMod("diceRoll", owner, x => x.HitBonus)
                .ActivityMod("target", "Base", targetDefence);

            var score = (abilityScore ?? initiator.MeleeAttack.Value) * (focusPoints + 1);

            if (abilityScore != null)
                activity.ActivityMod("diceRoll", "Ability", score);
            else
                activity.ActivityMod("diceRoll", "MeleeAttack.Value", score);

            return true;
        }

        public bool OnOutcome(Activity activity, MeleeWeapon owner, Actor initiator, int diceRoll, int target)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage)
                .ActivityMod("damage", initiator, x => x.Strength.Value);

            var meleeAttacking = initiator.CreateStateInstance(nameof(MeleeAttacking), new SpanOfTime(0, 1));
            activity.OutputSets.Add(meleeAttacking);

            return true;
        }
    }
}
