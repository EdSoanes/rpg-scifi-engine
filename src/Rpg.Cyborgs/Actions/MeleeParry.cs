using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeParry : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private MeleeParry() { }

        public MeleeParry(Actor owner)
            : base(owner)
        {
        }

        public bool OnCanAct(Actor owner)
            => !owner.IsStateOn(nameof(Parrying));

        public ModSet OnCost(Actor owner, Actor initiator, int focusPoints)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(owner, x => x.CurrentFocusPoints, -focusPoints)
                .Add(new TurnMod(1, 1), initiator, x => x.CurrentActionPoints, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, Actor owner, int target, int focusPoints, int? abilityScore)
        {
            var actionModSet = actionInstance.CreateActionSet()
                .DiceRoll(owner, "Base", "2d6")
                .Target(owner, "Target", target);

            if (abilityScore != null)
                actionModSet.DiceRoll(owner, "AbilityScore", abilityScore.Value);
            else
                actionModSet.DiceRoll(owner, x => x.MeleeAttack);

            return actionModSet;
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, Actor owner, int diceRoll, int target, int damage)
        {
            var parrying = owner.CreateStateInstance(nameof(Parrying), new TurnLifecycle(1, 1));
            var damageSet = actionInstance
                .CreateOutcomeSet()
                .Outcome(owner, "Damage", damage);

            //If successful parry...
            if (diceRoll >= target)
                damageSet.Outcome(owner, x => -x.Strength);

            return [parrying];
        }
    }
}
