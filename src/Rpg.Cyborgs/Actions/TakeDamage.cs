using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Actions
{
    public class TakeDamage : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private TakeDamage() { }

        public TakeDamage(Actor owner)
            : base(owner)
        {
        }

        public bool OnCanAct(RpgActivity activity, Actor owner)
            => (activity.GetActivityProp("damage") ?? Dice.Zero) != Dice.Zero;

        public bool OnCost(RpgActivity activity, Actor owner)
            => true;

        public bool OnAct(RpgActivity activity, Actor owner, int damage)
        {
            var staminaInjury = owner.CurrentStaminaPoints >= damage
                ? damage
                : owner.CurrentStaminaPoints;

            var lifeInjury = staminaInjury < damage
                ? damage - staminaInjury
                : 0;

            if (staminaInjury > 0)
                activity
                    .OutcomeSet.Add(new PermanentMod().SetBehavior(new Combine()), owner, x => x.CurrentStaminaPoints, -staminaInjury);

            if (lifeInjury > 0)
                activity
                    .OutcomeSet.Add(new PermanentMod().SetBehavior(new Combine()), owner, x => x.CurrentLifePoints, -staminaInjury);

            if (lifeInjury < 0)
            {
                var currentLifePoints = owner.CurrentLifePoints - lifeInjury;
                activity
                    .ActionMod("injuryRoll", "Base", "2d6")
                    .ActionMod("injuryRoll", "LifeInjury", -currentLifePoints);
            }

            return lifeInjury > 0;
        }

        public bool OnOutcome(RpgActivity activity, Actor owner, int injuryRoll, int injuryLocationRoll, int locationType)
        {
            var bodyPart = GetLocation(owner, injuryLocationRoll, locationType);

            //Add injuries to body part...

            return true;
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
