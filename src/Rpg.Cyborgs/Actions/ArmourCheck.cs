using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Actions
{
    public class ArmourCheck : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private ArmourCheck() { }

        public ArmourCheck(Actor owner)
            : base(owner)
        {
        }

        public bool OnCanAct(Actor owner)
            => owner.Wearing.Get<Armour>().Any();

        public ModSet OnCost(Actor owner, int luckPoints)
        {
            return new ModSet(owner.Id, new TurnLifecycle())
                .Add(owner, x => x.CurrentLuckPoints, -luckPoints);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, Actor owner, int damage, int diceRoll1, int diceRoll2, int luckPoints)
        {
            var armour = owner.Wearing.Get<Armour>()
                .OrderByDescending(x => x.CurrentArmourRating)
                .FirstOrDefault();

            var armourRating = armour?.CurrentArmourRating ?? 0;
            var reduction = CalculateDamageReduction(armourRating, diceRoll1, diceRoll2, luckPoints);

            var actionSet = actionInstance.CreateActionSet();
            switch (reduction)
            {
                case "success":
                    actionSet.DiceRoll(owner, "DamageReduction", damage);
                    break;
                case "partial":
                    actionSet.DiceRoll(owner, "DamageReduction", Convert.ToInt32(Math.Ceiling((double)damage / 2)));
                    break;
                default:
                    actionSet.DiceRoll(owner, "DamageReduction", 0);
                    break;
            }

            return actionSet;
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, Actor owner, int damage, int damageReduction)
        {
            var armour = owner.Wearing.Get<Armour>()
                .OrderByDescending(x => x.CurrentArmourRating)
                .FirstOrDefault();

            var damageSet = actionInstance.CreateOutcomeSet();
            if (damage > 0 && damageReduction > 0)
            {
                if (damageReduction == damage)
                    damageSet.Add(new PermanentMod().SetBehavior(new Combine()), armour!, x => x.CurrentArmourRating, -2);
                else
                    damageSet.Add(new PermanentMod().SetBehavior(new Combine()), armour!, x => x.CurrentArmourRating, -1);
            }

            damageSet.Outcome(owner, "Damage", damage - damageReduction);

            return [damageSet];
        }

        private string CalculateDamageReduction(int armourRating, int diceRoll1, int diceRoll2, int luckPoints)
        {
            var success1 = luckPoints > 0 || diceRoll1 > armourRating;
            var success2 = luckPoints > 1 || diceRoll2 > armourRating;

            if (success1 && success2)
                return "success";

            if (!success1 && !success2)
                return "fail";

            return "partial";
        }
    }
}
