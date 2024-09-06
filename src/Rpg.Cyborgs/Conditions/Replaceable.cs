using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Conditions
{
    public class Replaceable : Condition<BodyPart>
    {
        [JsonConstructor] private Replaceable() { }

        public Replaceable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity >= (int)InjurySeverityEnum.Mangled;
    }
}
