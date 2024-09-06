using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Conditions
{
    public class Pain : Condition<BodyPart>
    {
        [JsonConstructor] private Pain() { }

        public Pain(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity == (int)InjurySeverityEnum.FleshWound;
    }
}
