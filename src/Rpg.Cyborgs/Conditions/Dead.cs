using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Dead : Condition<BodyPart>
    {
        [JsonConstructor] private Dead() { }

        public Dead(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => IsOn || (owner.BodyPartType == BodyPartType.Torso || owner.BodyPartType == BodyPartType.Head) && owner.InjurySeverity >= (int)InjurySeverityEnum.Severed;
    }
}
