using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection.Attributes;

namespace Rpg.Cyborgs.Actions
{
    public class TakeInjury : ActionTemplate<Actor>
    {
        [JsonConstructor] protected TakeInjury()
            : base() { }

        public TakeInjury(Actor owner)
            : base(owner) { }

        public bool Perform(ModObjects.Activities.Action action, Actor owner, int lifeInjury)
        {
            if (lifeInjury > 0)
            {
                action
                    .SetProp("injuryRoll", "2d6")
                    .SetProp("injuryRoll", -lifeInjury)
                    .SetProp("injuryLocationRoll", "1d6");
            }

            return true;
        }

        [ArgSelect(Arg = "locationType", Enum = typeof(InjuryLocationType))]
        public bool Outcome(ModObjects.Activities.Action action, Actor owner, int injuryRoll, int injuryLocationRoll, int locationType)
        {
            var injurySeverity = GetInjurySeverity(injuryRoll);
            var bodyPart = GetLocation(owner, injuryLocationRoll, locationType);

            action.OutcomeModSet.Add(new Permanent(), bodyPart, x => x.InjurySeverity, injurySeverity);

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
