using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Actions
{
    public class TakeDamage : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private TakeDamage() { }

        public TakeDamage(Actor owner)
            : base(owner)
        {
        }

        public bool OnCanAct(Actor owner)
            => true;

        public ModSet OnCost(int actionNo, Actor owner)
            => new ModSet(new TurnLifecycle());

        public ActionModSet OnAct(ActionInstance actionInstance, Actor owner, int damage)
        {
            var staminaDamage = owner.CurrentStaminaPoints >= damage
                ? damage
                : owner.CurrentStaminaPoints;

            var lifeDamage = staminaDamage < damage
                ? damage - staminaDamage
                : 0;

            var damageSet = actionInstance.CreateActionSet();

            if (staminaDamage > 0)
                damageSet.Add(new PermanentMod().SetBehavior(new Combine()), owner, x => x.CurrentStaminaPoints, -staminaDamage);

            if (lifeDamage > 0)
            {
                damageSet
                    .Add(new PermanentMod().SetBehavior(new Combine()), owner, x => x.CurrentLifePoints, -lifeDamage)
                    .DiceRoll(owner, "Base", "2d6")
                    .DiceRoll(owner, "CurrentLifePoints", owner.CurrentLifePoints - lifeDamage);
            }

            return damageSet;
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, Actor owner, int injuryRoll, int injuryLocationRoll, int locationType)
        {
            var bodyPart = GetLocation(owner, injuryLocationRoll, locationType);
            var actionSet = actionInstance.CreateActionSet();

            //Add injuries to body part...

            return [actionSet];
        }

        private RpgComponent GetLocation(Actor owner, int injuryLocationRoll, int locationType)
        {
            //random
            if (locationType == 0)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftLeg,
                    2 => owner.RightLeg,
                    3 => owner.LeftArm,
                    4 => owner.RightArm,
                    5 => owner.Torso,
                    _ => owner.Head
                };

            //High
            if (locationType == 1)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftArm,
                    2 => owner.RightArm,
                    3 => owner.Torso,
                    4 => owner.Torso,
                    5 => owner.Head,
                    _ => owner.Head
                };

            //Low
            if (locationType == 2)
                return injuryLocationRoll switch
                {
                    1 => owner.LeftLeg,
                    2 => owner.LeftLeg,
                    3 => owner.RightLeg,
                    4 => owner.RightLeg,
                    5 => owner.Torso,
                    _ => owner.Torso
                };

            return owner.Torso;
        }
    }
}
