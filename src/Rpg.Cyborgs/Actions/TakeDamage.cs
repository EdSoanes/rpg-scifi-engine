using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection.Attributes;
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

        public bool OnCanAct(Activity activity, Actor owner)
            => (activity.GetActivityProp("damage") ?? Dice.Zero) != Dice.Zero;

        public bool OnCost(Activity activity, Actor owner)
            => true;

        public bool OnAct(Activity activity, Actor owner, int damage)
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
                    .ActivityMod("injuryRoll", "Base", "2d6")
                    .ActivityMod("injuryRoll", "LifeInjury", -currentLifePoints)
                    .ActivityMod("injuryLocationRoll", "Base", "1d6");
            }

            return lifeInjury > 0;
        }

        [ArgSelect(Arg = "locationType", Enum = typeof(InjuryLocationType))]
        public bool OnOutcome(Activity activity, Actor owner, int injuryRoll, int injuryLocationRoll, int locationType)
        {
            var bodyPart = GetLocation(owner, injuryLocationRoll, locationType);
            var injurySeverity = GetInjurySeverity(injuryRoll);

            activity.OutcomeSet
                .Add(bodyPart, x => x.InjurySeverity, injurySeverity);

            return true;
        }

        private BodyPart GetLocation(Actor owner, int injuryLocationRoll, int locationType)
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

        private int GetInjurySeverity(int injuryRoll)
            => injuryRoll switch
            {
                <= 2 => 6,
                <= 4 => 5,
                <= 7 => 4,
                <= 10 => 3,
                <= 12 => 2,
                _ => 1
            };
    }
}
