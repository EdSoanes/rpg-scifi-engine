using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Bleeding : Condition<BodyPart>
    {
        [JsonConstructor] private Bleeding() { }

        public Bleeding(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Torso && owner.InjurySeverity == (int)InjurySeverityEnum.Busted
                || owner.BodyPartType == BodyPartType.Head && owner.InjurySeverity == (int)InjurySeverityEnum.Unusable
                || owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity >= (int)InjurySeverityEnum.Mangled;
    }
}
