using Newtonsoft.Json;
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

        public ModSet[] OnAct(int actionNo, Actor owner, int damage, int diceRoll1, int diceRoll2, int luckPoints)
        {
            var armour = owner.Wearing.Get<Armour>()
                .OrderByDescending(x => x.CurrentArmourRating)
                .FirstOrDefault();

            var armourRating = armour?.CurrentArmourRating ?? 0;
            var reduction = CalculateDamageReduction(armourRating, diceRoll1, diceRoll2, luckPoints);

            var reductionSet = new ModSet(owner.Id, new TurnLifecycle());
            switch (reduction)
            {
                case "success":
                    ActResult(actionNo, reductionSet, owner, "DamageReduction", damage);
                    reductionSet.Add(new PermanentMod(), armour!, x => x.CurrentArmourRating, -2);
                    break;
                case "partial":
                    ActResult(actionNo, reductionSet, owner, "DamageReduction", Convert.ToInt32(Math.Ceiling((double)damage / 2)));
                    reductionSet.Add(new PermanentMod(), armour!, x => x.CurrentArmourRating, -1);
                    break;

                default:
                    ActResult(actionNo, reductionSet, owner, "DamageReduction", 0);
                    break;
            }

            return [reductionSet];
        }

        public ModSet[] OnOutcome(int actionNo, Actor owner, int damage, int damageReduction)
        {
            var armour = owner.Wearing.Get<Armour>()
                .OrderByDescending(x => x.CurrentArmourRating)
                .FirstOrDefault();

            var damageSet = new ModSet(owner.Id, new TurnLifecycle());
            if (damage > 0 && damageReduction > 0)
            {
                if (damageReduction == damage)
                    damageSet.Add(new PermanentMod(), armour!, x => x.CurrentArmourRating, -2);
                else
                    damageSet.Add(new PermanentMod(), armour!, x => x.CurrentArmourRating, -1);
            }

            OutcomeMod(actionNo, damageSet, owner, "Damage", damage - damageReduction);

            return [damageSet];
        }

        public string CalculateDamageReduction(int armourRating, int diceRoll1, int diceRoll2, int luckPoints)
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
