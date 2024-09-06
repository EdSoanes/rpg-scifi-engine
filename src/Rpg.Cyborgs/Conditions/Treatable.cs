using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Conditions
{
    public class Treatable : Condition<BodyPart>
    {
        [JsonConstructor] private Treatable() { }

        public Treatable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity == (int)InjurySeverityEnum.Unusable;
    }
}
