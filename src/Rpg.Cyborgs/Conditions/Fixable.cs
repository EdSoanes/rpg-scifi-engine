using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Conditions
{
    public class Fixable : Condition<BodyPart>
    {
        [JsonConstructor] private Fixable() { }

        public Fixable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Busted;
    }
}
